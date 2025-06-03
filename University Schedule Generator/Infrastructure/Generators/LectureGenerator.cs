using Bogus;

namespace University_Schedule_Generator.Services.Generators;

public class LectureGenerator
{
    private readonly Faker _faker;

    // Список курсов больше не нужен в конструкторе для основного метода,
    // но может понадобиться для вспомогательного метода генерации случайной лекции.
    private readonly List<Course> _courses;

    // --- Компоненты для названий Лекций (Более разнообразные) ---
    private static readonly string[] _introThemes =
    {
        "Введение в", "Основные понятия", "Обзор дисциплины:", "Цели и задачи курса:", "Исторический экскурс:"
    };

    private static readonly string[] _coreThemes =
    {
        "Теоретические основы", "Ключевые концепции", "Методология", "Анализ", "Моделирование",
        "Принципы", "Структура", "Классификация", "Стандартизация", "Проектирование", "Разработка",
        "Тестирование", "Оптимизация", "Применение", "Практикум по теме:"
    };

    private static readonly string[] _advancedThemes =
    {
        "Современные тенденции в", "Сравнительный анализ", "Перспективные разработки",
        "Критические аспекты", "Нестандартные подходы к", "Сложные задачи в",
        "Исследование", "Будущее развитие"
    };

    private static readonly string[] _conclusionThemes =
    {
        "Заключение по курсу:", "Подведение итогов:", "Обзор пройденного материала:", "Перспективы изучения",
        "Итоговый обзор:"
    };

    // Примеры подтем (очень общие, можно детализировать по областям)
    private static readonly string[] _subTopicExamples =
    {
        "алгоритмов", "структур данных", "баз данных", "сетевых протоколов", "операционных систем",
        "квантовой механики", "термодинамики", "электромагнетизма", "органической химии", "генетики",
        "микроэкономических моделей", "макроэкономической политики", "финансовых рынков", "маркетинговых стратегий",
        "исторических периодов", "философских школ", "социологических теорий", "правовых норм",
        "лингвистических аспектов"
    };

    // --- Вспомогательный метод для парсинга ДИАПАЗОНА годов ---
    /// <summary>
    /// Извлекает начальный и конечный год из строки формата "YYYY-YYYY".
    /// </summary>
    /// <param name="term">Строка периода курса.</param>
    /// <returns>Кортеж (startYear, endYear). При ошибке возвращает (0, 0) или (startYear, startYear), если удалось распарсить только первый год.</returns>
    private (int startYear, int endYear) ParseYearRangeFromTerm(string term)
    {
        const int minValidYear = 1900;
        const int maxValidYear = 2100;

        if (string.IsNullOrWhiteSpace(term))
        {
            Console.Error.WriteLine(
                $"Предупреждение: Term курса пустой или null. Невозможно определить диапазон годов.");
            return (0, 0);
        }

        var parts = term.Split('-');
        if (parts.Length != 2) // Ожидаем ровно две части
        {
            // Попытка распарсить как один год, если нет дефиса
            if (parts.Length == 1 && int.TryParse(parts[0].Trim(), out int singleYear) && singleYear >= minValidYear &&
                singleYear <= maxValidYear)
            {
                Console.Error.WriteLine(
                    $"Предупреждение: Term '{term}' содержит только один год. Используется как начало и конец диапазона.");
                return (singleYear, singleYear);
            }

            Console.Error.WriteLine(
                $"Предупреждение: Неверный формат Term '{term}'. Ожидался 'ГГГГ-ГГГГ'. Невозможно определить диапазон годов.");
            return (0, 0);
        }

        bool startParsed = int.TryParse(parts[0].Trim(), out int startYear)
                           && startYear >= minValidYear && startYear <= maxValidYear;
        bool endParsed = int.TryParse(parts[1].Trim(), out int endYear)
                         && endYear >= minValidYear && endYear <= maxValidYear;

        if (startParsed && endParsed)
        {
            if (startYear <= endYear)
            {
                return (startYear, endYear); // Успешный парсинг диапазона
            }
            else
            {
                Console.Error.WriteLine(
                    $"Предупреждение: Начальный год ({startYear}) больше конечного ({endYear}) в Term '{term}'. Используется только начальный год.");
                return (startYear, startYear); // Диапазон некорректен, используем только первый
            }
        }
        else if (startParsed) // Если удалось распарсить только первый
        {
            Console.Error.WriteLine(
                $"Предупреждение: Не удалось распарсить конечный год в Term '{term}'. Используется только начальный год {startYear}.");
            return (startYear, startYear);
        }
        else // Если не удалось распарсить даже первый
        {
            Console.Error.WriteLine(
                $"Предупреждение: Не удалось распарсить начальный год в Term '{term}'. Невозможно определить год.");
            return (0, 0);
        }
    }

    // Конструктор может принимать список курсов для вспомогательных методов
    public LectureGenerator(Faker faker, IEnumerable<Course> courses = null)
    {
        _faker = faker ?? throw new ArgumentNullException(nameof(faker));
        // Сохраняем список, если он передан (для генерации случайной лекции)
        _courses = courses?.ToList();
    }

    // --- Основной метод: Генерация ПОСЛЕДОВАТЕЛЬНОСТИ лекций для КУРСА ---
    /// <summary>
    /// Генерирует список лекций для указанного курса.
    /// </summary>
    /// <param name="course">Курс, для которого генерируются лекции.</param>
    /// <param name="lectureCount">Количество лекций для генерации (рекомендуется 8-16).</param>
    /// <returns>Список сгенерированных объектов Lecture.</returns>
    public List<Lecture> GenerateLecturesForCourse(Course course)
    {
        if (course == null) throw new ArgumentNullException(nameof(course));

        // *** Извлекаем диапазон годов из Term курса ***
        var (startYear, endYear) = ParseYearRangeFromTerm(course.Term);

        int lectureCount = _faker.Random.Int(8, 16);
        var lectures = new List<Lecture>();
        var usedSubtopics = new HashSet<string>();

        for (int i = 0; i < lectureCount; i++)
        {
            // ... (логика генерации имени и requirements остается прежней) ...
            int lectureNumber = i + 1;
            string lectureName;
            string theme;
            string subTopic = "";
            // ... (выбор theme, subTopic, формирование lectureName) ...
            if (i == 0 && lectureCount > 1)
            {
                theme = _faker.PickRandom(_introThemes);
            }
            else if (i == lectureCount - 1 && lectureCount > 2)
            {
                theme = _faker.PickRandom(_conclusionThemes);
            }
            else
            {
                theme = _faker.PickRandom(_coreThemes.Concat(_advancedThemes));
                if (_faker.Random.Bool(0.6f) && _subTopicExamples.Any())
                {
                    subTopic = _faker.PickRandom(_subTopicExamples.Except(usedSubtopics));
                    if (!string.IsNullOrEmpty(subTopic)) usedSubtopics.Add(subTopic);
                    else subTopic = _faker.PickRandom(_subTopicExamples);
                }
            }

            string formattedLectureNumber = $"Лекция {lectureNumber}:";
            if (theme.EndsWith(":")) lectureName = $"{formattedLectureNumber} {theme.TrimEnd(':')} {subTopic}".Trim();
            else if (!string.IsNullOrEmpty(subTopic))
                lectureName = $"{formattedLectureNumber} {theme} {subTopic}".Trim();
            else
            {
                lectureName = $"{formattedLectureNumber} {theme}".Trim();
                if (i > 0 && i < lectureCount - 1 && _faker.Random.Bool(0.3f))
                    lectureName += $" (на примере курса \"{course.Name}\")";
            }

            lectureName = lectureName.Replace("  ", " ");

            float requirementProbability = 0.1f + ((float)lectureNumber / lectureCount) * 0.4f;
            bool requirements = _faker.Random.Float() < requirementProbability;


            // *** Генерируем случайный год из диапазона ***
            int lectureYear = 0; // Год по умолчанию, если диапазон не определен
            if (startYear > 0) // Проверяем, что хотя бы начальный год валиден
            {
                // Если endYear == startYear, Random.Int вернет startYear
                lectureYear = _faker.Random.Int(startYear, endYear);
            }

            // Создаем объект Lecture, добавляя СЛУЧАЙНЫЙ год из диапазона
            var newLecture = new Lecture
            {
                Name = lectureName,
                Requirements = requirements,
                CourseId = course.Id,
                Course = course,
                Year = lectureYear // Присваиваем сгенерированный год
            };
            lectures.Add(newLecture);
        }

        return lectures;
    }

    // --- Вспомогательный метод: Генерация ОДНОЙ случайной лекции для СЛУЧАЙНОГО курса ---
    /// <summary>
    /// Генерирует одну случайную лекцию для случайного курса из списка, переданного в конструктор.
    /// </summary>
    /// <returns>Объект Lecture или null, если список курсов не был предоставлен.</returns>
    public Lecture GenerateRandomLecture()
    {
        if (_courses == null || !_courses.Any())
        {
            Console.Error.WriteLine(
                "Предупреждение: Список курсов не был предоставлен, не могу сгенерировать случайную лекцию.");
            return null;
        }

        var selectedCourse = _faker.PickRandom(_courses);

        // *** Извлекаем диапазон годов из Term выбранного курса ***
        var (startYear, endYear) = ParseYearRangeFromTerm(selectedCourse.Term);

        // ... (логика генерации имени и requirements) ...
        string theme = _faker.PickRandom(_introThemes.Concat(_coreThemes).Concat(_advancedThemes));
        string subTopic = _faker.Random.Bool(0.5f) ? _faker.PickRandom(_subTopicExamples) : "";
        string lectureName;
        if (theme.EndsWith(":"))
        {
            lectureName = $"{theme.TrimEnd(':')} {subTopic}".Trim();
        }
        else if (!string.IsNullOrEmpty(subTopic))
        {
            lectureName = $"{theme} {subTopic}".Trim();
        }
        else
        {
            lectureName = $"{theme} \"{selectedCourse.Name}\"".Trim();
        }

        lectureName = lectureName.Replace("  ", " ");
        bool requirements = _faker.Random.Bool(0.25f);


        // *** Генерируем случайный год из диапазона ***
        int lectureYear = 0; // Год по умолчанию
        if (startYear > 0)
        {
            lectureYear = _faker.Random.Int(startYear, endYear);
        }

        // Создаем объект Lecture, добавляя СЛУЧАЙНЫЙ год из диапазона
        var newLecture = new Lecture
        {
            Name = lectureName,
            Requirements = requirements,
            CourseId = selectedCourse.Id,
            // Course = selectedCourse, // Можно раскомментировать, если нужно
            Year = lectureYear // Присваиваем сгенерированный год
        };

        return newLecture;
    }
}