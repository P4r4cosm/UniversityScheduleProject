using Bogus;
using University_Schedule_Generator.Interfaces.Generator;

namespace University_Schedule_Generator.Services.Generators;


// Генератор Кафедр (Departments)
    public class DepartmentGenerator: IDataGenerator
    {
        private readonly Faker _faker;
        private readonly List<Institute> _institutes; // Храним список доступных институтов

        // --- Компоненты для названий Кафедр (остаются как были) ---
        private static readonly string[] SubjectStemsGenitive =
        {
            // Технические / IT / Математика
            "высшей математики", "прикладной математики и информатики", "теоретической механики",
            "программной инженерии", "компьютерных систем и сетей", "информационной безопасности",
            "анализа данных и искусственного интеллекта", "робототехники и мехатроники",
            "теоретической физики", "экспериментальной физики", "оптики и лазерных технологий",
            "электроники и наноэлектроники", "радиотехники и систем связи",
            "электроэнергетики и электротехники", "теплоэнергетики", "материаловедения",
            "машиностроения", "автоматизации производственных процессов",
            "строительных конструкций", "архитектуры и градостроительства", "геодезии и картографии",
            "нефтегазового дела", "горного дела",

            // Естественные науки
            "общей и неорганической химии", "органической химии", "физической химии",
            "биологии и экологии", "генетики и биотехнологии", "географии и природопользования",
            "почвоведения и агрохимии",

            // Гуманитарные / Социальные / Экономика
            "русского языка и литературы", "иностранных языков", "лингвистики и перевода",
            "истории Отечества", "всеобщей истории", "археологии и этнологии",
            "философии и социальных наук", "социологии и политологии", "психологии",
            "педагогики и методики преподавания",
            "экономической теории", "финансов и кредита", "бухгалтерского учета и аудита",
            "менеджмента и маркетинга", "государственного и муниципального управления",
            "мировой экономики и международных отношений",
            "теории и истории государства и права", "гражданского права и процесса", "уголовного права и криминологии",
            "журналистики и медиакоммуникаций", "рекламы и связей с общественностью",

            // Искусство / Физкультура
            "теории и истории искусств", "дизайна", "музыкального образования",
            "теории и методики физического воспитания", "спортивных дисциплин"
        };

        private static readonly string[] OptionalPrefixes =
        {
            "Общей", "Специальной", "Фундаментальной", "Прикладной", "Теоретической"
        };
        // Конструктор принимает Faker и КОЛЛЕКЦИЮ Институтов
        public DepartmentGenerator(Faker faker, IEnumerable<Institute> institutes)
        {
            _faker=faker;
            // Копируем список и проверяем на null/пустоту
            _institutes = institutes?.ToList() ?? throw new ArgumentNullException(nameof(institutes));

            if (!_institutes.Any())
            {
                throw new ArgumentException("Список институтов не может быть пустым.", nameof(institutes));
            }
        }

        // Метод для генерации данных Кафедры (сам выбирает институт)
        public Department GenerateDepartmentData()
        {
            // 1. Выбираем случайный институт из списка
            var selectedInstitute = _faker.PickRandom(_institutes);
            
            // 2. Генерируем название кафедры
            string departmentName;
            string baseStem = _faker.PickRandom(SubjectStemsGenitive);

            // С небольшой вероятностью добавляем опциональный префикс
            if (_faker.Random.Bool(0.15f)) // 15% шанс
            {
                string optionalPrefix = _faker.PickRandom(OptionalPrefixes);
                // Убедимся, что не будет "Кафедра Общей общей химии"
                if (!baseStem.StartsWith(optionalPrefix.ToLowerInvariant()))
                {
                    departmentName = $"Кафедра {optionalPrefix.ToLower()} {baseStem}";
                }
                else
                {
                    departmentName = $"Кафедра {baseStem}"; // Если префикс уже есть, не добавляем
                }
            }
            else
            {
                departmentName = $"Кафедра {baseStem}";
            }

            // Небольшая постобработка (можно расширить)
            departmentName = departmentName.Replace("ая высшей", "ой высшей");
            departmentName = departmentName.Replace("ая теоретической", "ой теоретической");
            // Убираем возможное двойное "кафедра кафедра", если префикс был "кафедра"
            departmentName = departmentName.Replace("Кафедра кафедра", "Кафедра");


            // 3. Получаем ID выбранного института
            int instituteId = selectedInstitute.Id;

            // 4. Возвращаем результат
            return new Department(){Name = departmentName, Institute = selectedInstitute};
        }

        public string Generate()
        {
            var res = GenerateDepartmentData();
            return $"{res.InstituteId} {res.Name}";
        }
    }