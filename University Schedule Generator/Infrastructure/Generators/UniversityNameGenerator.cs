using Bogus;
using University_Schedule_Generator.Interfaces.Generator;

namespace University_Schedule_Generator.Services.Generators;

public class UniversityNameGenerator: IDataGenerator
{
    private readonly Faker _faker;

    public UniversityNameGenerator(Faker faker)
    {
        _faker = faker;
    }
    // --- Компоненты для названий ---

    // Прилагательные, часто указывающие на местоположение или масштаб
    private static readonly string[] LocationAdjectives =
    {
        "Московский", "Санкт-Петербургский", "Российский", "Казанский",
        "Новосибирский", "Уральский", "Сибирский", "Дальневосточный",
        "Южный", "Северный", "Балтийский", "Воронежский", "Самарский",
        "Нижегородский", "Томский", "Омский", "Красноярский", "Пермский",
        "Волгоградский", "Башкирский", "Кубанский"
        // Добавьте больше по желанию
    };

    // Типы университетов по профилю
    private static readonly string[] UniversityTypes =
    {
        "технический", "педагогический", "медицинский", "экономический",
        "гуманитарный", "аграрный", "политехнический", "технологический",
        "юридический", "финансовый", "классический", "лингвистический",
        "архитектурно-строительный", "транспортный", "культурный"
        // Добавьте больше по желанию
    };

    // Префиксы, указывающие на статус (часто пишутся с большой буквы)
    private static readonly string[] StatusPrefixes =
    {
        "Государственный", "Национальный исследовательский", "Федеральный"
    };

    // Основное слово
    private static readonly string[] InstitutionKinds =
    {
        "университет", "институт", "академия"
    };

    // Опционально: Имена известных личностей (в родительном падеже)
    private static readonly string[] NamedAfter =
    {
        "имени М.В. Ломоносова", "имени А.С. Пушкина", "имени Д.И. Менделеева",
        "имени И.М. Сеченова", "имени Г.В. Плеханова", "имени П.И. Чайковского",
        "имени И.П. Павлова", "имени К.А. Тимирязева", "имени Н.Э. Баумана"
        // Добавьте больше по желанию
    };
    
    public string Generate()
    {
        var nameParts = new List<string>();
        string chosenStatusPrefix = null;
        string chosenLocationAdj = null;
        string chosenType = null;

        // --- Собираем название по частям с элементами случайности ---

        // 1. Шанс добавить префикс статуса (например, "Государственный")
        if (_faker.Random.Bool(0.6f)) // 60% шанс
        {
            chosenStatusPrefix = _faker.PickRandom(StatusPrefixes);
            nameParts.Add(chosenStatusPrefix);
        }

        // 2. Добавить прилагательное местоположения (почти всегда, если нет статуса)
        // Или с шансом ~80%, если статус уже есть
        if (chosenStatusPrefix == null || _faker.Random.Bool(0.8f))
        {
            chosenLocationAdj = _faker.PickRandom(LocationAdjectives);

            // Небольшая логика, чтобы избежать "Федеральный Российский университет"
            if (!((chosenStatusPrefix == "Национальный исследовательский" || chosenStatusPrefix == "Федеральный")
                  && chosenLocationAdj == "Российский"))
            {
                nameParts.Add(chosenLocationAdj);
            }
            else if (nameParts.Count == 1) // Если был только префикс и "Российский" пропустили
            {
                // Добавим другое местоположение, чтобы имя не было слишком коротким
                nameParts.Add(_faker.PickRandom(LocationAdjectives.Where(l => l != "Российский")));
            }
        }

        // 3. Шанс добавить тип/профиль (например, "технический")
        if (_faker.Random.Bool(0.80f)) // 80% шанс
        {
            chosenType = _faker.PickRandom(UniversityTypes);
            // Избегаем "Государственный государственный университет"
            if (!(chosenStatusPrefix == "Государственный" && chosenType == "государственный"))
            {
                nameParts.Add(chosenType);
            }
        }

        // 4. Добавить основное слово (университет, институт, академия)
        // Сделаем университет более частым
        string kind =
            _faker.Random.WeightedRandom(InstitutionKinds,
                new float[] { 0.7f, 0.2f, 0.1f }); // 70% универ, 20% институт, 10% академия
        nameParts.Add(kind);

        // 5. Небольшой шанс добавить "имени кого-то"
        if (NamedAfter.Any() && _faker.Random.Bool(0.15f)) // 15% шанс
        {
            nameParts.Add(_faker.PickRandom(NamedAfter));
        }

        // --- Очистка и сборка ---

        // Удаляем возможные дубликаты (если случайно сгенерировалось)
        // и пустые строки, если логика усложнится
        var finalParts = new List<string>();
        if (nameParts.Count > 0)
        {
            finalParts.Add(nameParts[0]);
            for (int i = 1; i < nameParts.Count; i++)
            {
                if (!string.IsNullOrEmpty(nameParts[i]) &&
                    !nameParts[i]
                        .Equals(nameParts[i - 1], StringComparison.OrdinalIgnoreCase)) // Сравнение без учета регистра
                {
                    finalParts.Add(nameParts[i]);
                }
            }
        }


        // Гарантируем, что имя не пустое
        if (!finalParts.Any())
        {
            // Запасной вариант, если все шансы провалились
            return $"{_faker.PickRandom(LocationAdjectives)} {_faker.PickRandom(InstitutionKinds)}";
        }

        return string.Join(" ", finalParts);
    }
}