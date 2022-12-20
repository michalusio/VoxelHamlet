using LibNoise;
using System;

namespace Assets.General
{
    internal class Voxelized : ModuleBase
    {
        private readonly ModuleBase Input;
        private readonly int Steps;

        public Voxelized(int steps, ModuleBase input): base(1)
        {
            Input = input;
            Steps = steps;
        }

        public override double GetValue(double x, double y, double z)
        {
            var value = Input.GetValue(x, y, z);
            return Math.Round(value * Steps) / Steps;
        }
    }
}
