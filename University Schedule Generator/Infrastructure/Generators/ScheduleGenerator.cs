// --- Обновленный Генератор Расписания ---

using System.Globalization;
using Bogus;
using University_Schedule_Generator;

public class ScheduleGenerator
{
    private readonly Faker _faker;
    private int _nextScheduleId = 1;

    // Константы для определения границ академического года 
    private const int ACADEMIC_YEAR_START_MONTH = 9; // Сентябрь
    private const int ACADEMIC_YEAR_START_DAY = 1;
    private const int ACADEMIC_YEAR_END_MONTH = 6; // Июнь
    private const int ACADEMIC_YEAR_END_DAY = 30;

    public ScheduleGenerator(Faker faker)
    {
        _faker = faker ?? throw new ArgumentNullException(nameof(faker));
    }

    /// <summary>
    /// Парсит строку Term "YYYY-YYYY" в начальный и конечный год.
    /// </summary>
    private bool TryParseTerm(string term, out int startYear, out int endYear)
    {
        startYear = 0;
        endYear = 0;
        if (string.IsNullOrWhiteSpace(term)) return false;

        var parts = term.Split('-');
        if (parts.Length != 2) return false;

        // Используем InvariantCulture для парсинга годов
        if (!int.TryParse(parts[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out startYear) ||
            !int.TryParse(parts[1].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out endYear))
        {
            return false;
        }

        // Простая валидация
        return startYear > 1900 && endYear >= startYear && endYear < 3000;
    }

    /// <summary>
    /// Получает границы дат на основе академических годов.
    /// </summary>
    private (DateTime termStartDate, DateTime termEndDate) GetTermDateRange(int startYear, int endYear)
    {
        // Начало: 1 сентября начального года
        DateTime termStartDate = new DateTime(startYear, ACADEMIC_YEAR_START_MONTH, ACADEMIC_YEAR_START_DAY);
        // Конец: 30 июня конечного года (для Bogus.Date.Between, конечная дата не включается,
        // поэтому берем следующий день - 1 июля)
        DateTime termEndDate = new DateTime(endYear, ACADEMIC_YEAR_END_MONTH, ACADEMIC_YEAR_END_DAY).AddDays(1);

        return (termStartDate, termEndDate);
    }

    /// <summary>
    /// Находит пересечение двух диапазонов дат.
    /// </summary>
    private (DateTime finalStartDate, DateTime finalEndDate)? GetIntersection(DateTime start1, DateTime end1,
        DateTime start2, DateTime end2)
    {
        var finalStart = start1 > start2 ? start1 : start2; // Max(start1, start2)
        var finalEnd = end1 < end2 ? end1 : end2; // Min(end1, end2)

        if (finalStart >= finalEnd) // Если диапазоны не пересекаются или пересечение - точка
        {
            return null; // Невозможно найти дату в этом пересечении
        }

        return (finalStart, finalEnd);
    }


    /// <summary>
    /// Генерирует один объект Schedule для указанной группы и лекции,
    /// учитывая Term курса лекции.
    /// </summary>
    /// <param name="group">Группа.</param>
    /// <param name="lecture">Лекция (должна иметь ссылку на Course с валидным Term).</param>
    /// <param name="minDate">Минимальная желаемая дата начала (необязательно).</param>
    /// <param name="maxDate">Максимальная желаемая дата начала (необязательно).</param>
    /// <returns>Сгенерированный объект Schedule.</returns>
    /// <exception cref="ArgumentNullException">Если group, lecture или lecture.Course равны null.</exception>
    /// <exception cref="ArgumentException">Если Term курса некорректен или указанный диапазон дат не пересекается с Term.</exception>
    public Schedule Generate(Group group, Lecture lecture, DateTime? minDate = null, DateTime? maxDate = null)
    {
        if (group == null) throw new ArgumentNullException(nameof(group));
        if (lecture == null) throw new ArgumentNullException(nameof(lecture));
        if (lecture.Course == null)
            throw new ArgumentNullException(nameof(lecture.Course), "У лекции отсутствует ссылка на объект Course.");
        if (string.IsNullOrWhiteSpace(lecture.Course.Term))
            throw new ArgumentException("Свойство Term у Course не может быть пустым.", nameof(lecture.Course.Term));

        // 1. Парсим Term курса
        if (!TryParseTerm(lecture.Course.Term, out int startYear, out int endYear))
        {
            throw new ArgumentException(
                $"Некорректный формат Term ('{lecture.Course.Term}') у Course Id={lecture.Course.Id}. Ожидается 'YYYY-YYYY'.",
                nameof(lecture.Course.Term));
        }

        // 2. Определяем диапазон дат на основе Term
        var (termStartDate, termEndDate) = GetTermDateRange(startYear, endYear);

        // 3. Определяем запрошенный диапазон дат (если есть)
        // Используем MinValue/MaxValue как "бесконечность" если min/max не заданы
        DateTime requestedMinDate = minDate ?? DateTime.MinValue;
        DateTime requestedMaxDate = maxDate ?? DateTime.MaxValue;

        // Убедимся что maxDate не раньше minDate
        if (requestedMaxDate < requestedMinDate)
        {
            throw new ArgumentException("maxDate не может быть раньше minDate.", nameof(maxDate));
        }

        // Для Between конечная дата не включается, скорректируем если задана
        if (maxDate.HasValue) requestedMaxDate = requestedMaxDate.AddDays(1);


        // 4. Находим пересечение диапазона Term и запрошенного диапазона
        var intersection = GetIntersection(termStartDate, termEndDate, requestedMinDate, requestedMaxDate);

        if (!intersection.HasValue)
        {
            throw new ArgumentException(
                $"Запрошенный диапазон дат ({minDate?.ToShortDateString() ?? "N/A"} - {maxDate?.ToShortDateString() ?? "N/A"}) не пересекается с периодом курса ({termStartDate.ToShortDateString()} - {termEndDate.AddDays(-1).ToShortDateString()}) для лекции '{lecture.Name}'.");
        }

        var (finalStartDate, finalEndDate) = intersection.Value;

        // 5. Генерируем время начала лекции в пределах финального диапазона дат
        DateTime startTime;
        int attempts = 0; // Предохранитель от бесконечного цикла, если диапазон очень мал и содержит только выходные
        const int maxAttempts = 100;

        do
        {
            if (attempts++ > maxAttempts)
            {
                throw new InvalidOperationException(
                    $"Не удалось найти будний день в диапазоне {finalStartDate.ToShortDateString()} - {finalEndDate.AddDays(-1).ToShortDateString()} за {maxAttempts} попыток.");
            }

            DateTime baseDate = _faker.Date.Between(finalStartDate, finalEndDate);
            int hour = _faker.Random.Int(8, 16);
            int minute = _faker.PickRandom(0, 30);

            // !!! ИСПРАВЛЕНИЕ: Добавляем DateTimeKind.Utc !!!
            startTime = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, hour, minute, 0, DateTimeKind.Utc);
        } while (startTime.DayOfWeek == DayOfWeek.Saturday || startTime.DayOfWeek == DayOfWeek.Sunday);

// Время окончания автоматически получит Kind = Utc от startTime
        DateTime endTime = startTime.AddMinutes(90);

        // 6. Создаем объект Schedule
        var schedule = new Schedule
        {
            Lecture = lecture,
            Group = group,
            StartTime = startTime,
            EndTime = endTime
        };

        return schedule;
    }

    // --- Перегрузки метода Generate ---
    // (Они используют основной метод Generate, поэтому автоматически учтут Term)

    /// <summary>
    /// Генерирует список объектов Schedule для указанной группы, выбирая лекции из предоставленного списка.
    /// Учитывает Term каждой лекции.
    /// </summary>
    public List<Schedule> Generate(Group group, List<Lecture> availableLectures, int count, DateTime? minDate = null,
        DateTime? maxDate = null)
    {
        if (group == null) throw new ArgumentNullException(nameof(group));
        if (availableLectures == null) throw new ArgumentNullException(nameof(availableLectures));
        if (!availableLectures.Any())
            throw new ArgumentException("Список доступных лекций не может быть пустым.", nameof(availableLectures));
        if (count <= 0) return new List<Schedule>();

        var schedules = new List<Schedule>(count);
        int generationAttempts = 0;
        const int
            maxGenerationAttempts =
                5; // Сколько раз пытаться сгенерить одну запись, если выбранная лекция не подходит по датам

        while (schedules.Count < count && generationAttempts < count * maxGenerationAttempts)
        {
            generationAttempts++;
            Lecture randomLecture = _faker.PickRandom(availableLectures);
            try
            {
                // Пытаемся сгенерировать запись, используя основной метод, который проверит Term
                var scheduleEntry = Generate(group, randomLecture, minDate, maxDate);
                schedules.Add(scheduleEntry);
            }
            catch (ArgumentException ex)
            {
                // Игнорируем ошибку, если Term лекции не совпал с запрошенным диапазоном дат,
                // и пробуем выбрать другую лекцию на следующей итерации.
                Console.WriteLine(
                    $"Предупреждение: Не удалось сгенерировать расписание для лекции ID={randomLecture.Id} ({randomLecture.Name}) - {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Ошибка поиска буднего дня - возможно, диапазон слишком узкий
                Console.WriteLine(
                    $"Предупреждение: Не удалось сгенерировать расписание для лекции ID={randomLecture.Id} ({randomLecture.Name}) - {ex.Message}");
            }
        }

        if (schedules.Count < count)
        {
            Console.WriteLine(
                $"Предупреждение: Удалось сгенерировать только {schedules.Count} из {count} запрошенных записей расписания из-за ограничений Term или диапазона дат.");
        }

        return schedules;
    }

    /// <summary>
    /// Генерирует список объектов Schedule, выбирая случайные группы и лекции из предоставленных списков.
    /// Учитывает Term каждой лекции.
    /// </summary>
    public List<Schedule> Generate(List<Group> availableGroups, List<Lecture> availableLectures, int count,
        DateTime? minDate = null, DateTime? maxDate = null)
    {
        if (availableGroups == null) throw new ArgumentNullException(nameof(availableGroups));
        if (availableLectures == null) throw new ArgumentNullException(nameof(availableLectures));
        if (!availableGroups.Any())
            throw new ArgumentException("Список доступных групп не может быть пустым.", nameof(availableGroups));
        if (!availableLectures.Any())
            throw new ArgumentException("Список доступных лекций не может быть пустым.", nameof(availableLectures));
        if (count <= 0) return new List<Schedule>();

        var schedules = new List<Schedule>(count);
        int generationAttempts = 0;
        const int maxGenerationAttempts = 5; // Попыток на запись

        while (schedules.Count < count && generationAttempts < count * maxGenerationAttempts)
        {
            generationAttempts++;
            Group randomGroup = _faker.PickRandom(availableGroups);
            Lecture randomLecture = _faker.PickRandom(availableLectures);
            try
            {
                var scheduleEntry = Generate(randomGroup, randomLecture, minDate, maxDate);
                schedules.Add(scheduleEntry);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(
                    $"Предупреждение: Не удалось сгенерировать расписание для гр.{randomGroup.Id}/лекц.{randomLecture.Id} - {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(
                    $"Предупреждение: Не удалось сгенерировать расписание для гр.{randomGroup.Id}/лекц.{randomLecture.Id} - {ex.Message}");
            }
        }

        if (schedules.Count < count)
        {
            Console.WriteLine(
                $"Предупреждение: Удалось сгенерировать только {schedules.Count} из {count} запрошенных записей расписания.");
        }

        return schedules;
    }
}