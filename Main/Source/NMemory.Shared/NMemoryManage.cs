using System.Text;

namespace NMemory
{
    public static class NMemoryManager
    {
		public static bool UseDefaultForNotNullable { get; set; }

        public static bool UseTimestampBytesReverse { get; set; }

        /// <summary>
        /// Gets if the object cloning should be disabled. When disabled, the entity is directly stored and retrieved from the memory database
        /// </summary>
        public static bool DisableObjectCloning { get; set; }
    }
}
