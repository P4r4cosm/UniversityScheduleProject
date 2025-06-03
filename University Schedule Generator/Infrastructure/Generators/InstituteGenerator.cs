using Bogus;
using University_Schedule_Generator.Interfaces.Generator;

namespace University_Schedule_Generator.Services.Generators;


// Генератор Институтов, адаптированный под ваши классы
public class InstituteGenerator: IDataGenerator
{
    private readonly Faker _faker;
    private readonly List<University> _universities; // Храним список ваших University

    // --- Компоненты для названий институтов (Расширены для разнообразия) ---
    private static readonly string[] Prefixes =
    {
        "Институт", "Факультет", "Высшая школа", "Центр", "Департамент",
        "Научно-образовательный центр", "Институт высоких технологий", "Школа"
    };

    // Темы/Области знаний в родительном падеже (кого? чего?)
    private static readonly string[] SubjectStemsGenitive =
    {
        "информационных технологий", "программной инженерии", "кибербезопасности",
        "математики и компьютерных наук", "прикладной математики", "фундаментальной физики",
        "экономики и управления", "финансов и аудита", "маркетинга и логистики",
        "физико-технических исследований", "гуманитарных наук и искусств", "социальных коммуникаций",
        "иностранных языков и лингвистики", "переводоведения", "естественных наук", "химии и биотехнологий",
        "права и публичной политики", "искусств и дизайна", "медиа и журналистики",
        "международных отношений и регионоведения", "стратегического управления", "бизнеса и предпринимательства",
        "педагогики и психологии", "строительства и архитектуры", "транспорта и логистики",
        "машиностроения и робототехники", "энергетики и электротехники", "медицины и здравоохранения",
        "агроинженерии", "экологии и природопользования"
    };

    // Темы/Области как прилагательные/сложные прилагательные (какой?)
    private static readonly string[] SubjectAdjectives =
    {
        "Физико-математический", "Инженерно-экономический", "Историко-филологический",
        "Химико-биологический", "Юридический", "Медицинский", "Аграрный", "Педагогический",
        "Лингвистический", "Компьютерных наук", "Энергетический", "Строительный",
        "Транспортный", "Социально-гуманитарный", "Информационных систем"
    };

    // Дополнительные описательные прилагательные
    private static readonly string[] DescriptiveAdjectives =
    {
        "Высший", "Фундаментальный", "Прикладной", "Инновационный", "Международный",
        "Современных", "Перспективных", "Передовых"
    };

    // Имена для "имени кого-то" (в родительном падеже)
    private static readonly string[] NamedAfter =
    {
        "имени М.В. Ломоносова", "имени А.М. Бутлерова", "имени Д.И. Менделеева",
        "имени И.М. Губкина", "имени С.П. Королёва", "имени П.Л. Чебышева",
        "имени Л.Д. Ландау", "имени Н.И. Пирогова"
        // Добавьте больше по желанию
    };


    // Конструктор принимает Faker и КОЛЛЕКЦИЮ ваших University
    public InstituteGenerator(Faker faker, IEnumerable<University> universities)
    {
        _faker = faker;
        _universities = universities?.ToList() ?? throw new ArgumentNullException(nameof(universities));

        if (!_universities.Any())
        {
            throw new ArgumentException("Список университетов не может быть пустым.", nameof(universities));
        }
    }
    // Метод для генерации данных Института
    public Institute GenerateInstituteData()
    {
        // 1. Выбираем случайный университет из списка
        var selectedUniversity = _faker.PickRandom(_universities);

        // 2. Генерируем название института с использованием разных шаблонов
        string instituteName = GenerateDiverseInstituteName(selectedUniversity);

        // 3. Возвращаем результат с названием и ID университета
        return new Institute(){Name = instituteName, University = selectedUniversity};
    }

    public string Generate()
    {
        var res = GenerateInstituteData();
        return $"{res.UniversityId} {res.Name}";
    }
    // Вспомогательный метод для генерации разнообразных названий
    private string GenerateDiverseInstituteName(University university)
    {
        // Выбираем случайный шаблон генерации
        int patternChoice = _faker.Random.Int(1, 100); // Используем 1-100 для процентного распределения

        if (patternChoice <= 45) // 45% шанс: Префикс + Основа (Род. падеж)
        {
            string prefix = _faker.PickRandom(Prefixes);
            string stem = _faker.PickRandom(SubjectStemsGenitive);
            return $"{prefix} {stem}";
        }

        if (patternChoice <= 65) // 20% шанс: Прилагательное + Префикс-существительное
        {
            string adjStem = _faker.PickRandom(SubjectAdjectives);
            // Выбираем только существительные ("Институт", "Факультет", "Центр", "Департамент", "Школа")
            string nounPrefix = _faker.PickRandom(Prefixes.Where(p => !p.Contains(" "))); // Убираем сложные префиксы
            return $"{adjStem} {nounPrefix}";
        }

        if (patternChoice <= 80) // 15% шанс: Префикс + Описательное прил. + Основа (Род. падеж)
        {
            string prefix = _faker.PickRandom(Prefixes.Where(p => !p.Contains(" "))); // Простой префикс
            string descAdj = _faker.PickRandom(DescriptiveAdjectives);
            string stem = _faker.PickRandom(SubjectStemsGenitive);
            // Простая проверка, чтобы избежать "Высший Высшая школа"
            if (prefix.StartsWith("Высшая", StringComparison.OrdinalIgnoreCase) &&
                descAdj.StartsWith("Высш", StringComparison.OrdinalIgnoreCase))
            {
                prefix = "Институт"; // Замена для избежания тавтологии
            }

            return $"{prefix} {descAdj} {stem}";
        }

        if (patternChoice <= 90 && NamedAfter.Any()) // 10% шанс: Префикс + "имени Кого-то"
        {
            string prefix = _faker.PickRandom(Prefixes);
            string named = _faker.PickRandom(NamedAfter);
            // Небольшая чистка, чтобы избежать "Институт информационных технологий имени М.В. Ломоносова" - лучше просто "Институт имени..."
            if (prefix.Contains(" ")) // Если префикс сложный, делаем его простым
            {
                prefix = "Институт";
            }

            return $"{prefix} {named}";
        }
        else // 10% шанс (или если список NamedAfter пуст): Название на основе университета
        {
            string prefix = _faker.PickRandom(Prefixes);
            string stem = _faker.PickRandom(SubjectStemsGenitive);
            // Очень упрощенный вариант - можно добавить логику извлечения части имени университета
            return $"{prefix} {stem} {university.Name}"; // Может выглядеть громоздко, но добавляет связь
            // Альтернатива:
            // return $"{university.Name.Split(' ')[0]} {prefix} {stem}"; // Использовать первое слово из названия университета
        }
    }
}