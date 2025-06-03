namespace University_Schedule_Lab1;

public class Material
{
    public int Id {get; set;}
    
    public string Name {get; set;}
    
    public int LectureId {get; set;}
    public Lecture Lecture {get; set;}
}