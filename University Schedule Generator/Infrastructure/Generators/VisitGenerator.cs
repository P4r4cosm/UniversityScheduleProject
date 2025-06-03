
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace University_Schedule_Generator
{
    public class VisitGenerator
    {
        private readonly Faker _faker;
        private int _nextVisitId = 1; // Счетчик для ID посещений

        public VisitGenerator(Faker faker)
        {
            _faker = faker ?? throw new ArgumentNullException(nameof(faker));
        }

        /// <summary>
        /// Генерирует записи посещений для списка студентов на основе предоставленного расписания их группы.
        /// </summary>
        /// <param name="studentsInGroup">Список студентов, принадлежащих к одной группе.</param>
        /// <param name="groupSchedule">Список записей расписания для группы этих студентов.</param>
        /// <param name="presenceProbability">Вероятность того, что студент будет присутствовать на занятии (от 0.0 до 1.0).</param>
        /// <param name="minArrivalOffsetMinutes">Минимальное смещение времени прихода относительно начала пары (отрицательное значение - пришел раньше).</param>
        /// <param name="maxArrivalOffsetMinutes">Максимальное смещение времени прихода относительно начала пары (положительное - опоздал).</param>
        /// <returns>Список сгенерированных объектов Visit.</returns>
        /// <exception cref="ArgumentNullException">Если studentsInGroup или groupSchedule равны null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Если presenceProbability вне диапазона [0, 1].</exception>
        public List<Visit> Generate(
            List<Student> studentsInGroup,
            List<Schedule> groupSchedule,
            float presenceProbability = 0.60f, // 60% шанс присутствия по умолчанию
            int minArrivalOffsetMinutes = 0,  // Может прийти за 0 минут до начала
            int maxArrivalOffsetMinutes = 40)   // Может опоздать на 40 минут
        {
            if (studentsInGroup == null) throw new ArgumentNullException(nameof(studentsInGroup));
            if (groupSchedule == null) throw new ArgumentNullException(nameof(groupSchedule));
            if (presenceProbability < 0.0f || presenceProbability > 1.0f)
                throw new ArgumentOutOfRangeException(nameof(presenceProbability), "Вероятность должна быть между 0.0 и 1.0.");
             if (maxArrivalOffsetMinutes < minArrivalOffsetMinutes)
                 throw new ArgumentException("Максимальное смещение не может быть меньше минимального.", nameof(maxArrivalOffsetMinutes));


            var visits = new List<Visit>();

            if (!studentsInGroup.Any() || !groupSchedule.Any())
            {
                return visits; // Нечего генерировать, если нет студентов или расписания
            }

            // Проверка, что все студенты из одной группы (опционально, но полезно)
            // var groupId = studentsInGroup.First().IdGroup;
            // if (studentsInGroup.Any(s => s.IdGroup != groupId))
            // {
            //     throw new ArgumentException("Все студенты в списке должны принадлежать одной группе.", nameof(studentsInGroup));
            // }
            // Проверка, что все расписание для той же группы (опционально)
            // if (groupSchedule.Any(sch => sch.GroupId != groupId))
            // {
            //     throw new ArgumentException("Все записи расписания должны принадлежать той же группе, что и студенты.", nameof(groupSchedule));
            // }


            foreach (var scheduleEntry in groupSchedule)
            {
                foreach (var student in studentsInGroup)
                {
                    // 1. Определяем, был ли студент на паре
                    bool attended = _faker.Random.Bool(presenceProbability);

                    if (attended)
                    {
                        // 2. Если был, определяем время прихода (VisitTime)
                        int arrivalOffset = _faker.Random.Int(minArrivalOffsetMinutes, maxArrivalOffsetMinutes);
                        DateTime visitTime = scheduleEntry.StartTime.AddMinutes(arrivalOffset);

                        // Опционально: убедимся, что время визита не позже конца пары
                        // (хотя опоздание на 20 мин при паре 90 мин - это нормально)
                        if (visitTime > scheduleEntry.EndTime)
                        {
                            // Можно просто установить время на конец пары или немного раньше
                            visitTime = scheduleEntry.EndTime.AddMinutes(-1);
                            // Или можно пропустить эту отметку, считая, что опоздал слишком сильно
                            // continue;
                        }
                         // Опционально: убедимся, что время визита не раньше определенного порога до начала
                         if (visitTime < scheduleEntry.StartTime.AddMinutes(minArrivalOffsetMinutes - 5)) // Доп. буфер
                         {
                            // Можно установить на минимальное время прихода
                             visitTime = scheduleEntry.StartTime.AddMinutes(minArrivalOffsetMinutes);
                         }


                        // 3. Создаем запись о посещении
                        var visit = new Visit
                        {
                            Student = student,
                            Schedule = scheduleEntry,
                            VisitTime = visitTime
                        };
                        visits.Add(visit);
                    }
                    // Если !attended, то запись Visit для этого студента и этой пары не создается
                }
            }

            return visits;
        }

         /// <summary>
        /// Генерирует записи посещений для одного студента на основе предоставленного расписания его группы.
        /// </summary>
        /// <param name="student">Студент.</param>
        /// <param name="groupSchedule">Список записей расписания для группы этого студента.</param>
        /// <param name="presenceProbability">Вероятность присутствия.</param>
        /// <param name="minArrivalOffsetMinutes">Мин. смещение времени прихода.</param>
        /// <param name="maxArrivalOffsetMinutes">Макс. смещение времени прихода.</param>
        /// <returns>Список сгенерированных объектов Visit для данного студента.</returns>
         public List<Visit> Generate(
            Student student,
            List<Schedule> groupSchedule,
            float presenceProbability = 0.6f,
            int minArrivalOffsetMinutes = 0,
            int maxArrivalOffsetMinutes = 40)
         {
             if (student == null) throw new ArgumentNullException(nameof(student));
             // Вызываем основной метод, передав список из одного студента
             return Generate(new List<Student> { student }, groupSchedule, presenceProbability, minArrivalOffsetMinutes, maxArrivalOffsetMinutes);
         }
    }
}