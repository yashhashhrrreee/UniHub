using System.IO;

using NUnit.Framework;

namespace UnitTests
{

    /// <summary>
    /// TextFixture is a necessary class for starting up unit tests
    /// </summary>
    [SetUpFixture]

    public class TestFixture
    {

        /// <summary>
        /// Path to the Web Root
        /// </summary>
        public static string DataWebRootPath = "./wwwroot";


        /// <summary>
        /// Path to the data folder for the content
        /// </summary>
        public static string DataContentRootPath = "./data/";


        /// <summary>
        /// Test Database
        /// </summary>
        public static string DataTestFile = "products_test.json";


        /// <summary>
        /// Production Database
        /// </summary>
        public static string DataProductionFile = "products.json";


        /// <summary>
        /// Net version (change when .net has different version)
        /// </summary>
        public static string NetVersion = "net8.0";


        /// <summary>
        /// Runs once before any of the unit tests are executed.
        /// Sets up a static copy of the test database so tests run against a consistent dataset.
        /// </summary>
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {

            // Run this code once when the test harness starts up.

            // This will copy the test database to the new location for the production
            // database. The reason is because when the code starts it reacts to the 
            // production database (products.json). If we copy the test database so
            // it uses the production database name, then the unit tests will work on 
            // a static representation, so that it should pass. The production database
            // might be dynamic for instance.

            var DataWebPath = Path.Combine("..", "..", "..", "..", "src", "wwwroot", "data");

            var DataUTDirectory = "wwwroot";

            var DataUTPath = DataUTDirectory + "/data";

            // Delete the Destination folder
            if (Directory.Exists(DataUTDirectory))
            {
                Directory.Delete(DataUTDirectory, true);
            }

            // Make the directory
            Directory.CreateDirectory(DataUTPath);


            // Copy over all data files from old path to new path
            var FilePaths = Directory.GetFiles(DataWebPath);

            foreach (var Filename in FilePaths)
            {
                string OriginalFilePathName = Filename.ToString();

                var NewFilePathName = OriginalFilePathName.Replace(DataWebPath, DataUTPath);

                File.Copy(OriginalFilePathName, NewFilePathName);
            }

        }


        /// <summary>
        /// RunAfterAnyTests will contain activities you do after each test
        /// </summary>
        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
        }

    }
}