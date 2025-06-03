using System.Text.RegularExpressions;
using Bogus;

namespace University_Schedule_Generator.Services.Generators;

public class MaterialGenerator
{
    private readonly Faker _faker;

    // --- Компоненты для названий Материалов (остаются как были) ---
    private static readonly string[] _materialTypes = {
        "Презентация", "Конспект", "Слайды", "Дополнительные материалы", "Задание",
        "Практическое задание", "Примеры кода", "Рекомендуемая литература",
        "Видеозапись", "Методические указания", "Тестовые вопросы"
    };

    private static readonly string[] _connectingPhrases = {
        "к лекции", "по теме", "для лекции", "к занятию", "по материалам лекции"
    };

    // Конструктор теперь принимает только Faker
    public MaterialGenerator(Faker faker)
    {
        _faker = faker ?? throw new ArgumentNullException(nameof(faker));
    }

    // Метод для генерации 1-2 материалов для КОНКРЕТНОЙ лекции
    /// <summary>
    /// Генерирует список из 1 или 2 материалов для указанной лекции.
    /// </summary>
    /// <param name="lecture">Лекция, для которой генерируются материалы.</param>
    /// <returns>Список сгенерированных объектов Material (1 или 2 элемента).</returns>
    public List<Material> GenerateMaterialsForLecture(Lecture lecture)
    {
        if (lecture == null)
        {
            throw new ArgumentNullException(nameof(lecture));
        }

        int numberOfMaterials = _faker.Random.Int(1, 2); // Определяем, сколько материалов генерировать (1 или 2)
        var generatedMaterials = new List<Material>(numberOfMaterials); // Создаем список нужного размера

        for (int i = 0; i < numberOfMaterials; i++)
        {
            // Генерируем Название материала, используя переданную лекцию
            string materialName = GenerateSingleMaterialName(lecture);

            // Создаем объект Material и заполняем поля
            var newMaterial = new Material
            {
                Name = materialName,
                Lecture = lecture 
            };
            generatedMaterials.Add(newMaterial);
        }

        return generatedMaterials;
    }

    // Вспомогательный приватный метод для генерации ОДНОГО названия материала
    private string GenerateSingleMaterialName(Lecture selectedLecture)
    {
        string materialType = _faker.PickRandom(_materialTypes);
        string materialName;
        string lectureNumberStr = ExtractLectureNumber(selectedLecture.Name);
        string lectureTheme = ExtractLectureTheme(selectedLecture.Name);
        int patternChoice = _faker.Random.Int(1, 4);

        switch (patternChoice)
        {
            case 1 when !string.IsNullOrEmpty(lectureNumberStr):
                materialName = $"{materialType} к лекции {lectureNumberStr}";
                break;
            case 2:
                materialName = $"{materialType} по теме \"{lectureTheme}\"";
                break;
            case 3 when !string.IsNullOrEmpty(lectureNumberStr):
                 if (materialType.ToLower().Contains("задание") || materialType.ToLower().Contains("практическ"))
                     materialName = $"{materialType} №{lectureNumberStr}";
                 else
                      materialName = $"{materialType} {lectureNumberStr}";
                 break;
             case 4:
             default:
                 string phrase = _faker.PickRandom(_connectingPhrases);
                 materialName = string.IsNullOrEmpty(lectureNumberStr)
                     ? $"{materialType} {phrase} \"{lectureTheme}\""
                     : $"{materialType} {phrase} {lectureNumberStr}";
                 break;
        }
        return materialName.Replace("  ", " ").Trim();
    }


    // Вспомогательные методы для извлечения информации из названия лекции (остаются как были)
    private string ExtractLectureNumber(string lectureName)
    {
        if (string.IsNullOrWhiteSpace(lectureName)) return null;
        var match = Regex.Match(lectureName, @"Лекция\s*(?:№|#)?\s*(\d+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private string ExtractLectureTheme(string lectureName)
    {
         if (string.IsNullOrWhiteSpace(lectureName)) return "Неизвестная тема";
         var theme = Regex.Replace(lectureName, @"^Лекция\s*(?:№|#)?\s*\d+\s*[:\-]?\s*", "", RegexOptions.IgnoreCase).Trim();
         return string.IsNullOrWhiteSpace(theme) || theme.Length < 5 ? lectureName : theme;
    }
}
