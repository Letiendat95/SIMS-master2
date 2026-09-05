using System.Text;

namespace SIMS.Web.Services.CsvImport
{
    // Tách một dòng CSV thành các trường, có xử lý:
    //   - dấu phẩy nằm trong ô được bao bởi dấu nháy kép:  "Hanoi, Vietnam"
    //   - dấu nháy kép thoát bằng cách nhân đôi:            "He said ""hi"""
    // Viết tay thay vì String.Split(',') vì Split sẽ cắt nhầm các ô có dấu phẩy bên trong.
    public static class CsvLineSplitter
    {
        public static string[] Split(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // "" bên trong ô đang mở nháy = một dấu nháy kép thực sự
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                        inQuotes = true;
                    else if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                    }
                    else
                        current.Append(c);
                }
            }

            fields.Add(current.ToString());
            return fields.ToArray();
        }
    }
}
