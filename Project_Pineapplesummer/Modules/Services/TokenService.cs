using System.IO;

namespace Project_Pineapplesummer.Modules.Services
{
    internal class TokenService
    {
        //You could just use File.ReadAllText but this way you could add encryption
        internal string GetToken(string filename)
        {
            try
            {
                return File.ReadAllText($@"..\..\..\Modules\Services\{filename}.txt");
            }
            catch(FileNotFoundException ex)
            {
                ErrorServices es = new ErrorServices();
                es.SendErrorMessage(ex.Message, "COBOL compiler", ErrorServices.severity.Error);
                return "";
            }
        }
    }
}
