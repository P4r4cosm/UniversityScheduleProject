// Структура для возвращаемого значения генератором Студента

using System.ComponentModel.DataAnnotations.Schema;
using University_Schedule_Generator;
using Bogus;
using Bogus.DataSets;
using University_Schedule_Generator.Interfaces.Generator;

public record StudentGenerationResult(
    string FullName,
    DateTime DateOfRecipient, // Дата зачисления/начала обучения в группе
    int GroupId           // ID группы, к которой относится студент
);

// Генератор Студентов
public class StudentGenerator: IDataGenerator
{
    private readonly Faker _faker;
    private readonly List<Group> _groups; // Храним список доступных групп

    // Конструктор принимает Faker и КОЛЛЕКЦИЮ Групп
    public StudentGenerator(Faker faker, IEnumerable<Group> groups)
    {
        _faker = faker ?? throw new ArgumentNullException(nameof(faker));
        _groups = groups?.ToList() ?? throw new ArgumentNullException(nameof(groups));

        if (!_groups.Any())
        {
            throw new ArgumentException("Список групп не может быть пустым.", nameof(groups));
        }
    }

    // --- Логика генерации ФИО (вспомогательные методы) ---
    private string GenerateMalePatronymic(string fatherFirstName = null)
    {
        if (string.IsNullOrEmpty(fatherFirstName))
        {
            fatherFirstName = _faker.Name.FirstName(Name.Gender.Male);
        }

        // Правила остаются те же
        if (fatherFirstName.EndsWith("й") || fatherFirstName.EndsWith("ь")) {
             // Исключения для -ий/-ь -> евич
             if (fatherFirstName.EndsWith("ий"))
                return fatherFirstName.Substring(0, fatherFirstName.Length - 2) + "ьевич"; // Валерий -> Валерьевич
              return fatherFirstName.Substring(0, fatherFirstName.Length - 1) + "евич"; // Игорь -> Игоревич
        }
        else if (fatherFirstName.EndsWith("а") || fatherFirstName.EndsWith("я"))
        {
             if (fatherFirstName.EndsWith("а") && fatherFirstName.Length > 2 && "гкхчшщ".Contains(fatherFirstName[fatherFirstName.Length-2])) {
                 return fatherFirstName.Substring(0, fatherFirstName.Length - 1) + "ич"; // Лука -> Лукич
             }
             if (fatherFirstName == "Илья") return "Ильич";
             if (fatherFirstName == "Никита") return "Никитич";
             if (fatherFirstName == "Фома") return "Фомич";
             if (fatherFirstName == "Кузьма") return "Кузьмич";
             // Общий случай для остальных -а/-я -> -ич
             if (fatherFirstName.EndsWith("а") || fatherFirstName.EndsWith("я"))
                 return fatherFirstName.Substring(0, fatherFirstName.Length - 1) + "ич"; // Савва -> Саввич? (спорно, но как упрощение)

            // Если дошли сюда, это не стандартное -а/-я имя для -ич, используем -ович (маловероятно)
             return fatherFirstName + "ович";
        }
        else
        {
             // Проверка на гласную перед последней согласной для -евич/-ович
            if (fatherFirstName.Length > 1 && "аеёиоуыэюя".Contains(char.ToLower(fatherFirstName[fatherFirstName.Length - 2]))) {
                // Если гласная перед согласной, но не "й/ь" (например, Лев, Павел?) - сложный случай. Оставим -ович как базовый.
                 // return fatherFirstName + "евич"; // Нужно больше правил
            }
            return fatherFirstName + "ович"; // Иван -> Иванович, Петр -> Петрович
        }
    }

    private string GenerateFemalePatronymic(string fatherFirstName = null)
    {
        if (string.IsNullOrEmpty(fatherFirstName))
        {
            fatherFirstName = _faker.Name.FirstName(Name.Gender.Male);
        }

        // Правила остаются те же
        if (fatherFirstName.EndsWith("й") || fatherFirstName.EndsWith("ь"))
        {
             if (fatherFirstName.EndsWith("ий"))
                 return fatherFirstName.Substring(0, fatherFirstName.Length - 2) + "ьевна"; // Валерий -> Валерьевна
            return fatherFirstName.Substring(0, fatherFirstName.Length - 1) + "евна"; // Игорь -> Игоревна
        }
        else if (fatherFirstName.EndsWith("а") || fatherFirstName.EndsWith("я"))
        {
             // Илья -> Ильинична, Никита -> Никитична, Фома -> Фоминична, Кузьма -> Кузьминична
             if (fatherFirstName == "Илья") return "Ильинична";
             if (fatherFirstName == "Никита") return "Никитична";
             if (fatherFirstName == "Фома") return "Фоминична";
             if (fatherFirstName == "Кузьма") return "Кузьминична";

            // Для имен на -а/-я с шипящим/г/к/х -> -ична
            if (fatherFirstName.EndsWith("а") && fatherFirstName.Length > 2 && "гкхчшщ".Contains(fatherFirstName[fatherFirstName.Length-2])) {
                 return fatherFirstName.Substring(0, fatherFirstName.Length - 1) + "ична"; // Лука -> Лукична
            }

             // Общий случай для остальных -а/-я -> -ична
             if (fatherFirstName.EndsWith("а") || fatherFirstName.EndsWith("я"))
                 return fatherFirstName.Substring(0, fatherFirstName.Length - 1) + "ична"; // Савва -> Саввична?

            // Если дошли сюда, используем -овна
            return fatherFirstName + "овна";
        }
        else
        {
             // Как и в мужском, проверка на гласную перед согласной
             // if (...) return fatherFirstName + "евна";
             return fatherFirstName + "овна"; // Иван -> Ивановна, Петр -> Петровна
        }
    }

    // --- Основной метод генерации данных Студента ---
    public Student GenerateStudentData()
    {
        // 1. Выбираем случайную группу из списка
        var selectedGroup = _faker.PickRandom(_groups);

        // 2. Генерируем ФИО
        var gender = _faker.PickRandom<Name.Gender>();
        string fatherFirstName = _faker.Name.FirstName(Name.Gender.Male);
        string firstName = _faker.Name.FirstName(gender);
        string lastName = _faker.Name.LastName(gender);
        string patronymic = (gender == Name.Gender.Male)
            ? GenerateMalePatronymic(fatherFirstName)
            : GenerateFemalePatronymic(fatherFirstName);
        string fullName = $"{firstName} {patronymic} {lastName}";

        // 3. Определяем дату зачисления (DateOfRecipient)
        // Логично предположить, что это дата начала обучения группы
        DateTime dateOfRecipient = selectedGroup.StartYear;
        // Альтернатива: случайная дата вскоре после начала обучения
        // DateTime dateOfRecipient = _faker.Date.Between(selectedGroup.StartYear, selectedGroup.StartYear.AddMonths(1));

        // 4. Получаем ID выбранной группы
        int groupId = selectedGroup.Id;

        // 5. Возвращаем результат
        return new Student(){FullName = fullName, Group = selectedGroup, DateOfRecipient = dateOfRecipient};
    }

    public string Generate()
    {
        var student = GenerateStudentData();
        return $"{student.FullName} {student.DateOfRecipient} {student.GroupId}";
    }
}