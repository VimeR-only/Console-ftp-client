namespace WinSCP_DEV;
using WinSCP;

class Program
{
    private static void PrintFileTransfer(object sender, FileTransferProgressEventArgs e)
    {
        Console.WriteLine(e.FileName);
    }

    static string CurrentDirectory = "/";
    static string LastDirectory = "";
    static List<string> History = new List<string>();

    static void GoToDirectory(Session session, string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        RemoteDirectoryInfo directory = session.ListDirectory(path);
        RemoteFileInfo[] files = directory.Files .Where(f => f.Name != "." && f.Name != "..").ToArray();

        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"[{i}] " + files[i].Name);
        }

        Console.WriteLine("Чтобы вернуться назад напишите -1.");
        Console.Write("Выберите директорию: ");

        int index = Convert.ToInt16(Console.ReadLine());

        if (index == -1)
        {
            if (LastDirectory != null)
            {
                if (History.Count == 0) 
                {
                    GoToDirectory(session, CurrentDirectory);

                    return;
                }


                string _path = History.Last();

                History.RemoveAt(History.Count - 1);

                CurrentDirectory = _path;

                GoToDirectory(session, CurrentDirectory);
            }
            else
            {
                Console.WriteLine("Нельзя вернуться назад.");
                return;
            }
        }
        else
        {
            if (index >= 0 && index < files.Length)
            {
                LastDirectory = CurrentDirectory;

                History.Add(LastDirectory);

                CurrentDirectory = files[index].FullName;

                GoToDirectory(session, files[index].FullName);
            }
            else
            {
                Console.WriteLine("Директория не найден.");
            }
        }
    }
    static int Main(string[] args)
    {
        try
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = "",
                PortNumber = 2022,
                UserName = "",
                Password = "",
                SshHostKeyFingerprint = "",
            };

            using (Session session = new Session())
            {
                Console.WriteLine("1. Просмотр директорий.");
                Console.Write("Выберите действие: ");

                string ?operation = Console.ReadLine();

                session.Open(sessionOptions);

                switch (operation)
                {
                    case "1":
                        Console.WriteLine("Просмотр директорий.");

                        Console.Write("Введите путь: ");

                        string ?path = Console.ReadLine();

                        if (string.IsNullOrEmpty(path))
                        {
                            Console.WriteLine("Получен базовый путь.");

                            GoToDirectory(session, "/");
                        }
                        else
                        {
                            if (session.FileExists(path))
                            {
                                Console.WriteLine("Директория существует.");

                                CurrentDirectory = path;

                                GoToDirectory(session, path);   
                            }
                            else
                            {
                                Console.WriteLine("Путь не существует.");
                            }
                        }

                        break;
                    default:
                        Console.WriteLine("Такого действия нет.");
                        break;
                }
            }

            return 0;  
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: {0}", ex);
            return 1;
        }
    }
}