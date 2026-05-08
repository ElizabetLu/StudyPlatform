using StudyPlatform.Menus;

class Program
{
    static async Task Main()
    {
        var mainMenu = new MainMenu();

        await mainMenu.StartAsync();
    }
}