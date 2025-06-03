using Bogus;
using University_Schedule_Generator.Interfaces.Generator;

namespace University_Schedule_Generator.Services.Generators;

public class GroupGenerator: IDataGenerator
{
    private readonly Faker _faker;
    private readonly List<Department> _departments; // Храним список доступных кафедр

    // --- Компоненты для названий Групп ---
    // Типичные аббревиатуры (могут зависеть от специальности/направления)
    // Это ОЧЕНЬ примерный список, его нужно адаптировать под ваши реалии
    private static readonly string[] Abbreviations =
    {
        "ИВТ", "ПИН", "БИН", "ПМ", "ФТ", "ХТ", "БТ", "ЭН", "МХ", "АСУ", // Технические
        "ЭК", "МН", "БУ", "ФК", "ГМУ", "ТД", "ЛГ", // Экономика/Управление
        "ЮР", "ПД", "ПСО", // Юриспруденция
        "ИСТ", "ФЛ", "ЛИН", "ЖУР", "РСО", "ПС", "СОЦ", // Гуманитарные/Социальные
        "ПЕД", "ДО", "ДЕФ", // Педагогика
        "АРХ", "ДИЗ", "СТР", // Строительство/Искусство
        "АГР", "ЛХ", // Сельское хоз-во
        "СД", // Мед (Сестринское дело)
        // Можно добавить префиксы Б-бакалавр, М-магистр, С-специалист, А-аспирант
        // Но для простоты пока без них, т.к. формат дан как "БСБО-01-22"
        // "БСБО" вероятно Бакалавр Специальность Бизнес Образование? Слишком специфично.
        // Используем более общие аббревиатуры выше.
    };

    // Конструктор принимает Faker и КОЛЛЕКЦИЮ Кафедр (Departments)
    public GroupGenerator(Faker faker, IEnumerable<Department> departments)
    {
        _faker = faker;
        _departments = departments?.ToList() ?? throw new ArgumentNullException(nameof(departments));

        if (!_departments.Any())
        {
            throw new ArgumentException("Список кафедр не может быть пустым.", nameof(departments));
        }
    }

    // Метод для генерации данных Группы
    public Group GenerateGroupData()
    {
        // 1. Выбираем случайную кафедру из списка
        var selectedDepartment = _faker.PickRandom(_departments);

        // 2. Генерируем год поступления (например, за последние 5 лет)
        int currentYear = DateTime.Now.Year;
        int admissionYear = currentYear - _faker.Random.Int(0, 4); // Год поступления 0-4 года назад
        string yearSuffix = (admissionYear % 100).ToString("00"); // Последние 2 цифры года

        // 3. Генерируем даты начала и конца обучения (примерно 4 года для бакалавриата)
        DateTime startDate = new DateTime(admissionYear, 9, 1); // 1 сентября года поступления
        int graduationYear = admissionYear + 4; // Год окончания
        DateTime endDate = new DateTime(graduationYear, 6, 30); // 30 июня года окончания

        // 4. Генерируем название группы
        string abbreviation = _faker.PickRandom(Abbreviations);
        string sequenceNumber = _faker.Random.Int(1, 3).ToString("00"); // Номер группы (1-3 в потоке)

        string groupName = $"{abbreviation}-{sequenceNumber}-{yearSuffix}";

        // 5. Получаем ID выбранной кафедры
        int departmentId = selectedDepartment.Id;

        // 6. Возвращаем результат
        return new Group(){Name = groupName, Department = selectedDepartment, StartYear = startDate, EndYear = endDate};
    }

    public string Generate()
    {
        var group = GenerateGroupData();
        return $"{group.Name}, {group.DepartmentId}, {group.StartYear}, {group.EndYear}";
    }
}