using System.IO;

class Program
{
    static void Main()
    {
        string path = "Assets/Scripts/Editor/RoomSceneBuilder.cs";
        string[] lines = File.ReadAllLines(path);
        
        lines[22] = "        [MenuItem(\"Tools/💾 Save Current Room Layout as Custom Template\")]";
        lines[46] = "        [MenuItem(\"Tools/📂 Restore Saved Custom Room Template (Nạp Phòng Đã Lưu)\")]";
        lines[80] = "        [MenuItem(\"Tools/📦 Build Spacious Room (16m x 14m)\")]";
        lines[86] = "        [MenuItem(\"Tools/🎛️ Custom Room Builder Window (Tùy Chỉnh Kích Thước)\")]";
        lines[606] = "        [MenuItem(\"Tools/🔗 Link 3D Models to 30 ItemData Assets\")]";
        
        File.WriteAllLines(path, lines, System.Text.Encoding.UTF8);
    }
}
