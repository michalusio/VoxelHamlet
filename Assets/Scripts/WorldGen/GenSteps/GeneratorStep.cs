
namespace Assets.Scripts.WorldGen.GenSteps
{
    public interface IGeneratorStep
    {
        void Commit(CubeMap map);
    }
}
