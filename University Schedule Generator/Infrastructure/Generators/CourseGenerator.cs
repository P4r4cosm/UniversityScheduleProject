using Bogus;

namespace University_Schedule_Generator.Services.Generators;

// Генератор Курсов (Course), который связывает их с Кафедрами и Специальностями
public class CourseGenerator
{
    private readonly Faker _faker;
    private readonly List<Department> _departments; // Храним список доступных кафедр
    private readonly List<Speciality> _specialities; // Храним список доступных специальностей

    // --- Компоненты для названий Курсов (остаются как были) ---
    private static readonly string[] _courseNames = {
        // ... (ваш список названий курсов) ...
        "Математический анализ", "Линейная алгебра и аналитическая геометрия",
        "Дифференциальные уравнения", "Теория вероятностей и математическая статистика",
        "Дискретная математика", "Введение в программирование", "Базы данных",
        "Общая физика: Механика", "Электротехника и электроника",
        "Общая и неорганическая химия", "Органическая химия", "Биология",
        "Экономическая теория", "Микроэкономика", "Бухгалтерский учет и анализ",
        "История России", "Философия", "Иностранный язык (Английский)",
        "Правоведение", "Безопасность жизнедеятельности", "Физическая культура и спорт"
    };

    // Конструктор принимает Faker и КОЛЛЕКЦИИ Кафедр и Специальностей
    public CourseGenerator(Faker faker, IEnumerable<Department> departments, IEnumerable<Speciality> specialities)
    {
        _faker = faker ?? throw new ArgumentNullException(nameof(faker));

        // Копируем списки и проверяем на null/пустоту
        _departments = departments?.ToList() ?? throw new ArgumentNullException(nameof(departments));
        _specialities = specialities?.ToList() ?? throw new ArgumentNullException(nameof(specialities));

        if (!_departments.Any())
        {
            throw new ArgumentException("Список кафедр не может быть пустым.", nameof(departments));
        }
        if (!_specialities.Any())
        {
            throw new ArgumentException("Список специальностей не может быть пустым.", nameof(specialities));
        }
    }

    // Метод для генерации объекта Course с заполненными внешними ключами
    public Course GenerateCourse()
    {
        // 1. Выбираем случайную Кафедру
        var selectedDepartment = _faker.PickRandom(_departments);

        // 2. Выбираем случайную Специальность
        var selectedSpeciality = _faker.PickRandom(_specialities);

        // 3. Генерируем Название курса
        string courseName = _faker.PickRandom(_courseNames);

        // 4. Генерируем Учебный год (Термин)
        int currentYear = DateTime.Now.Year;
        int startYear = _faker.Random.Int(currentYear - 2, currentYear);
        int endYear = startYear + 1;
        string term = $"{startYear}-{endYear}";

        // 5. Создаем объект Course и заполняем все необходимые поля
        var newCourse = new Course
        {
            Name = courseName,
            Term = term,
            Department = selectedDepartment, 
            Speciality = selectedSpeciality
        };

        // 6. Возвращаем созданный объект
        return newCourse;
    }
}
