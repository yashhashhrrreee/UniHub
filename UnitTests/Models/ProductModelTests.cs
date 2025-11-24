using NUnit.Framework;
using ContosoCrafts.WebSite.Models;
using System.Text.Json;

namespace UnitTests.Pages
{
    /// <summary>
    /// Tests for ProductModel to exercise constructor defaults and ToString() serialization.
    /// </summary>
    [TestFixture]
    public class ProductModelTests
    {
        #region Constructor Tests

        /// <summary>
        /// Tests that default property values are correctly set when a ProductModel is instantiated.
        /// </summary>
        [Test]
        public void ProductModel_Constructor_NoParameters_DefaultValuesAssigned_ExpectDefaults()
        {
            // Arrange
            var ProductData = new ProductModel();

            // Act
            // (nothing to act on; verifying defaults)
            var Result = ProductData;

            // Reset
            // (no resources to reset for this test)

            // Assert
            // Id should be assigned and not empty
            Assert.AreNotEqual(string.Empty, Result.Id);

            // String defaults
            Assert.AreEqual(string.Empty, Result.Maker);
            Assert.AreEqual(string.Empty, Result.Location);
            Assert.AreEqual(string.Empty, Result.Image);
            Assert.AreEqual(string.Empty, Result.Url);
            Assert.AreEqual(string.Empty, Result.Title);
            Assert.AreEqual(string.Empty, Result.Description);

            // Arrays should be initialized empty
            Assert.IsNotNull(Result.Ratings);
            Assert.AreEqual(0, Result.Ratings.Length);

            Assert.IsNotNull(Result.GraduateDegree);
            Assert.AreEqual(0, Result.GraduateDegree.Length);

            Assert.IsNotNull(Result.UnderGraduateDegree);
            Assert.AreEqual(0, Result.UnderGraduateDegree.Length);
        }

        #endregion

        #region Serialization Tests

        /// <summary>
        /// Tests that ProductModel can be serialized to JSON and deserialized back correctly.
        /// </summary>
        [Test]
        public void ProductModel_ToString_PopulatedModel_SerializesAndDeserializes_ExpectEqualValues()
        {
            // Arrange
            var ProductData = new ProductModel
            {
                Maker = "Contoso",
                Location = "Earth",
                Image = "/images/p.png",
                Url = "https://example.com",
                Title = "Title",
                Description = "Desc",
                Ratings = new int[] { 1, 2, 3 },
                GraduateDegree = new string[] { "MS" },
                UnderGraduateDegree = new string[] { "BS" }
            };

            // Act
            var JsonResult = ProductData.ToString();

            // Reset
            // (no persistent state to reset)

            // Assert
            Assert.IsNotNull(JsonResult);

            var DeserializedModel = JsonSerializer.Deserialize<ProductModel>(JsonResult);
            Assert.IsNotNull(DeserializedModel);

            Assert.AreEqual(ProductData.Maker, DeserializedModel.Maker);
            Assert.AreEqual(ProductData.Location, DeserializedModel.Location);
            Assert.AreEqual(ProductData.Image, DeserializedModel.Image);
            Assert.AreEqual(ProductData.Url, DeserializedModel.Url);
            Assert.AreEqual(ProductData.Title, DeserializedModel.Title);
            Assert.AreEqual(ProductData.Description, DeserializedModel.Description);

            // Collections - use CollectionAssert for arrays
            CollectionAssert.AreEqual(ProductData.Ratings, DeserializedModel.Ratings);
            CollectionAssert.AreEqual(ProductData.GraduateDegree, DeserializedModel.GraduateDegree);
            CollectionAssert.AreEqual(ProductData.UnderGraduateDegree, DeserializedModel.UnderGraduateDegree);
        }

        #endregion

        #region Enum Deserialization Tests

        /// <summary>
        /// Tests deserialization of ProductTypeEnum from various JSON values (string, number, empty) correctly.
        /// </summary>
        [Test]
        public void ProductModel_TypeOfUniversity_JsonDeserialization_VariousTokens_ExpectMappedValues()
        {
            // Empty string -> Undefined
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":\"\"}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Reset
                // (no persistent state)

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Undefined, DeserializedModel.TypeOfUniversity);
            }

            // Named value -> Public
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":\"Public\"}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Public, DeserializedModel.TypeOfUniversity);
            }

            // Numeric string matching Private (5)
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":\"5\"}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Private, DeserializedModel.TypeOfUniversity);
            }

            // Number token matching Online (10)
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":10}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Online, DeserializedModel.TypeOfUniversity);
            }

            // Undefined numeric -> falls back to Undefined
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":999}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Undefined, DeserializedModel.TypeOfUniversity);
            }
        }

        #endregion

        #region Enum Serialization Tests

        /// <summary>
        /// Tests that serialization of ProductTypeEnum writes enum names as strings.
        /// </summary>
        [Test]
        public void ProductModel_TypeOfUniversity_Serialize_Community_ExpectNameString()
        {
            // Arrange
            var ProductData = new ProductModel()
            {
                TypeOfUniversity = ProductTypeEnum.Community
            };

            // Act
            var JsonResult = System.Text.Json.JsonSerializer.Serialize<ProductModel>(ProductData);

            // Reset
            // (no persistent state)

            // Assert
            Assert.IsTrue(JsonResult.Contains("\"TypeOfUniversity\":\"Community\""));
        }

        #endregion

        #region Edge Cases Tests

        /// <summary>
        /// Tests additional deserialization edge cases for ProductTypeEnum to ensure Undefined fallback works.
        /// </summary>
        [Test]
        public void ProductModel_TypeOfUniversity_JsonDeserialization_EdgeCases_ExpectUndefinedOrMapped()
        {
            // Numeric string that is not defined -> Undefined
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":\"999\"}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Undefined, DeserializedModel.TypeOfUniversity);
            }

            // Boolean token -> should fall back to Undefined
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":true}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Undefined, DeserializedModel.TypeOfUniversity);
            }

            // Large number token that cannot be parsed to int -> Undefined
            {
                // Arrange
                var data = "{\"TypeOfUniversity\":2147483648}"; // Int32.MaxValue + 1

                // Act
                var result = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(data);

                // Assert
                Assert.AreNotEqual(null, result);
                Assert.AreEqual(ProductTypeEnum.Undefined, result.TypeOfUniversity);
            }

            // Numeric string representing a defined numeric -> maps to Community
            {
                // Arrange
                var data = "{\"TypeOfUniversity\":\"18\"}"; // 18 exists (Community)

                // Act
                var result = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(data);

                // Assert
                Assert.AreNotEqual(null, result);
                Assert.AreEqual(ProductTypeEnum.Community, result.TypeOfUniversity);
            }
        }

        /// <summary>
        /// Tests that all ProductTypeEnum values serialize correctly to their string names.
        /// </summary>
        [Test]
        public void ProductModel_TypeOfUniversity_Serialize_AllEnumValues_ExpectNameStrings()
        {
            // Map of enum values to expected serialized string
            var Expectations = new (ProductTypeEnum value, string expected)[]
            {
                (ProductTypeEnum.Public, "Public"),
                (ProductTypeEnum.Private, "Private"),
                (ProductTypeEnum.Community, "Community"),
                (ProductTypeEnum.Online, "Online"),
                (ProductTypeEnum.Other, "Other"),
                (ProductTypeEnum.Undefined, "Undefined")
            };

            foreach (var (EnumValue, ExpectedString) in Expectations)
            {
                // Arrange
                var ProductData = new ProductModel() { TypeOfUniversity = EnumValue };

                // Act
                var JsonResult = System.Text.Json.JsonSerializer.Serialize<ProductModel>(ProductData);

                // Assert
                Assert.IsTrue(JsonResult.Contains($"\"TypeOfUniversity\":\"{ExpectedString}\""),
                    $"Expected serialized JSON to contain \"{ExpectedString}\" for enum value {EnumValue}. JSON: {JsonResult}");
            }
        }

        /// <summary>
        /// Tests deserialization of ProductTypeEnum for missing or unusual branches to ensure proper Undefined fallback.
        /// </summary>
        [Test]
        public void ProductModel_TypeOfUniversity_JsonDeserialization_MissingBranches_ExpectUndefinedOrMapped()
        {
            // Non-numeric, non-enum string -> Undefined
            {
                // Arrange
                var data = "{\"TypeOfUniversity\":\"NotANumber\"}";

                // Act
                var result = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(data);

                // Assert
                Assert.AreNotEqual(null, result);
                Assert.AreEqual(ProductTypeEnum.Undefined, result.TypeOfUniversity);
            }

            // Numeric string that maps to enum via numeric -> Other
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":\"+24\"}"; // Other = 24; plus sign forces Enum.TryParse to fail so int.TryParse path is used

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Other, DeserializedModel.TypeOfUniversity);
            }

            // Decimal number token -> Undefined
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":1.5}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Undefined, DeserializedModel.TypeOfUniversity);
            }

            // Null token -> Undefined
            {
                // Arrange
                var JsonData = "{\"TypeOfUniversity\":null}";

                // Act
                var DeserializedModel = System.Text.Json.JsonSerializer.Deserialize<ProductModel>(JsonData);

                // Assert
                Assert.AreNotEqual(null, DeserializedModel);
                Assert.AreEqual(ProductTypeEnum.Undefined, DeserializedModel.TypeOfUniversity);
            }
        }

        #endregion
    }
}