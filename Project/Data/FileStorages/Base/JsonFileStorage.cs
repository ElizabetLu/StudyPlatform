using System.Text.Json;

namespace StudyPlatform.Data.FileStorages.Base
{
    public class JsonFileStorage<T>
    {
        private readonly string _filePath;

        public JsonFileStorage(string filePath)
        {
            _filePath = filePath;
        }


        // 1. Reads all data from the JSON file and returns a List<T>.

        public async Task<List<T>> ReadAllAsync()
        {
            List<T> result = new List<T>();

            if (File.Exists(_filePath))

            {
                // does not blocks the program while reading data is in process -
                // the thread can be used for something else.

                string json = await File.ReadAllTextAsync(_filePath); 

                if (!string.IsNullOrWhiteSpace(json))

                {
                    // converts the JSON text into a List<T>.
                    // if the left value = null, use the right value.

                    result = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
                }
            }

            return result;
        }


        // 2. Converting the list into the JSON text and saving it.

        public async Task WriteAllAsync(List<T> items)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            string json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}