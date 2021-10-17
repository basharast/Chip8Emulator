namespace Chip8.Core
{
    public class Register16Bit
    {
        int _value;

        public int Value { get { return _value; } }

        public void Set(int value)
        {
            _value = value;
        }
    }
}