using System.Text;
using Bogus;
using University_Schedule_Generator;

public class MaterialElasticGenerator
{
    private readonly Faker _faker;

    public MaterialElasticGenerator(Faker faker)
    {
        _faker = faker ?? throw new ArgumentNullException(nameof(faker));
    }

    /// <summary>
    /// Преобразует объект Material в MaterialElastic и генерирует для него Content.
    /// </summary>
    /// <param name="inputMaterial">Исходный объект Material.</param>
    /// <param name="minParagraphs">Минимальное количество параграфов в Content.</param>
    /// <param name="maxParagraphs">Максимальное количество параграфов в Content.</param>
    /// <returns>Новый объект MaterialElastic с сгенерированным Content.</returns>
    /// <exception cref="ArgumentNullException">Если inputMaterial равен null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Если min/maxParagraphs некорректны.</exception>
    public MaterialElastic Generate(Material inputMaterial, int minParagraphs = 5, int maxParagraphs = 12)
    {
        if (inputMaterial == null) throw new ArgumentNullException(nameof(inputMaterial));
        if (minParagraphs <= 0 || maxParagraphs < minParagraphs)
            throw new ArgumentOutOfRangeException(nameof(minParagraphs), "Некорректное количество параграфов.");

        // 1. Генерируем Content (логика похожа на предыдущий генератор)
        var contentBuilder = new StringBuilder();

        // Используем имя из входного материала для заголовка
        contentBuilder.AppendLine($"## {inputMaterial.Name}");
        contentBuilder.AppendLine($"*{_faker.Lorem.Sentence(6, 10)}*"); // Вступление
        contentBuilder.AppendLine();

        int paragraphCount = _faker.Random.Int(minParagraphs, maxParagraphs);
        contentBuilder.AppendLine(_faker.Lorem.Paragraphs(paragraphCount, separator: "\n\n"));
        contentBuilder.AppendLine();

        // Добавляем случайные "несвязные" элементы
        if (_faker.Random.Bool(0.5f)) // 50% шанс добавить "факты"
        {
            contentBuilder.AppendLine("Дополнительные факты:");
            int facts = _faker.Random.Int(2, 5);
            for (int i = 0; i < facts; i++)
            {
                contentBuilder.AppendLine(
                    $"- {_faker.Rant.Review()}"); // Используем Rant.Review для случайных утверждений
            }

            contentBuilder.AppendLine();
        }

        if (_faker.Random.Bool(0.3f)) // 30% шанс добавить код (псевдо)
        {
            contentBuilder.AppendLine("```"); // Начало блока кода
            contentBuilder.AppendLine(_faker.Hacker.Verb() + " " + _faker.Hacker.Noun() + "();");
            contentBuilder.AppendLine($"// {_faker.Hacker.Phrase()}");
            contentBuilder.AppendLine("```"); // Конец блока кода
            contentBuilder.AppendLine();
        }

        contentBuilder.AppendLine("---");
        contentBuilder.AppendLine(
            $"*Источник: {_faker.Internet.DomainName()} ({_faker.Date.Recent().ToShortDateString()})*"); // Псевдо-источник

        // 2. Создаем объект MaterialElastic
        var elasticMaterial = new MaterialElastic
        {
            // Копируем данные из inputMaterial
            Id = inputMaterial.Id,
            Name = inputMaterial.Name,
            LectureId = inputMaterial.LectureId,
            // Lecture = inputMaterial.Lecture, // Копируем ссылку, если поле есть в MaterialElastic

            // Присваиваем сгенерированный контент
            Content = contentBuilder.ToString()
        };

        return elasticMaterial;
    }

    /// <summary>
    /// Преобразует список объектов Material в список MaterialElastic, генерируя Content для каждого.
    /// </summary>
    /// <param name="inputMaterials">Список исходных объектов Material.</param>
    /// <param name="minParagraphs">Мин. параграфов для каждого.</param>
    /// <param name="maxParagraphs">Макс. параграфов для каждого.</param>
    /// <returns>Список новых объектов MaterialElastic.</returns>
    public List<MaterialElastic> Generate(List<Material> inputMaterials, int minParagraphs = 5, int maxParagraphs = 12)
    {
        if (inputMaterials == null) throw new ArgumentNullException(nameof(inputMaterials));

        return inputMaterials.Select(mat => Generate(mat, minParagraphs, maxParagraphs)).ToList();
    }
}