namespace Discord.REST.Login
{
    [TestClass]
    public class LoginTests
    {
        [TestMethod]
        public void Login()
        {
            Console.WriteLine(Globals.RestClient.User.Username);
        }
    }
}
