namespace BlockFactory.Registry_;

public interface IRegistry
{
    public void AssignNumericalIds(Dictionary<string, int> mapping);
    public Dictionary<string, int> GetStringToNumIdMapping();
}