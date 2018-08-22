using System;
using System.IO;

namespace org.healthwise.ops.nugetsync
{
    public class PersistentDateTimeCursor
    {
        private readonly string _fileName;

        public PersistentDateTimeCursor(string fileName)
        {
            _fileName = fileName;
        }

        public DateTime Value
        {
            get => File.Exists(_fileName) 
                ? DateTime.Parse(File.ReadAllText(_fileName)) 
                : DateTime.MinValue;
            
            set => File.WriteAllText(_fileName, value.ToString("O"));
        }
    }
}