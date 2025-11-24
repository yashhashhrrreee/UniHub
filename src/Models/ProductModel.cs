using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContosoCrafts.WebSite.Models
{
    public enum ProductTypeEnum
    {
        /// <summary>
        /// Default value, representing an unselected or unknown state.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// A public university.
        /// </summary>
        Public = 1,

        /// <summary>
        /// A private university.
        /// </summary>
        Private = 5,

        /// <summary>
        /// A community college.
        /// </summary>
        Community = 18,

        /// <summary>
        /// An online-only institution.
        /// </summary>
        Online = 10,

        /// <summary>
        /// Other types of institutions.
        /// </summary>
        Other = 24
    }

    /// <summary>
    /// Product model used to represent product data loaded from JSON.
    /// </summary>
    public class ProductModel
    {
        /// <summary>
        /// Default constructor sets sensible defaults to avoid nulls.
        /// </summary>
        public ProductModel()
        {
            Id = Guid.NewGuid().ToString();
            Maker = string.Empty;
            Location = string.Empty;
            Image = string.Empty;
            Url = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Ratings = Array.Empty<int>();
            GraduateDegree = Array.Empty<string>();
            UnderGraduateDegree = Array.Empty<string>();
            TypeOfUniversity = ProductTypeEnum.Undefined;
            Campuses = new List<string>();

            // Use a valid default that satisfies the Range attribute.
            NumberOfDepartments = 1;
            HasOnlinePrograms = false;
        }

        // Unique identifier for the product.
        public string Id
        {
            get;
            set;
        }

        // Manufacturer or maker of the product.
        public string Maker
        {
            get;
            set;
        }

        // Location of the product.
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(55, ErrorMessage = "Location cannot exceed 55 characters.")]
        public string Location
        {
            get;
            set;
        }

        // Map JSON property "img" to Image. Accepts only local uploaded images saved under /images.
        [JsonPropertyName("img")]
        // Only accept local images that live under /images with png/jpg/jpeg extensions.
        // The upload handler will save the uploaded file into /images and set this
        // property to a value like "/images/filename.png" before persisting.
        [RegularExpression(@"(?i)(^\/images\/.+\.(png|jpg|jpeg)$)", ErrorMessage = "Image must be a local /images path (png/jpg/jpeg).")]
        public string Image
        {
            get;
            set;
        }

        // Link to the product page or external URL.
        [Required(ErrorMessage = "Website URL is required.")]
        [Url(ErrorMessage = "Enter a valid URL starting with https://")]
        [RegularExpression(@"^https:\/\/.*", ErrorMessage = "Website URL must start with https://")]
        public string Url
        {
            get;
            set;
        }

        // Display title of the product.
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(55, ErrorMessage = "Title cannot exceed 55 characters.")]
        public string Title
        {
            get;
            set;
        }

        // Short description of the product.
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description
        {
            get;
            set;
        }

        // Array of integer ratings (e.g., per-review or stars).
        public int[] Ratings
        {
            get;
            set;
        }

        // JSON property mapping for graduate degree list.
        [JsonPropertyName("graduateDegree")]
        public string[] GraduateDegree
        {
            get;
            set;
        }

        // JSON property mapping for undergraduate degree list.
        [JsonPropertyName("undergraduateDegree")]
        public string[] UnderGraduateDegree
        {
            get;
            set;
        }

        // Type of the university (e.g., Public, Private, Community, Online).
        // Stored as an enum (serialized/deserialized as a string in JSON).
        [Required(ErrorMessage = "Type of University is required.")]
        [EnumDataType(typeof(ProductTypeEnum))]
        [JsonConverter(typeof(ProductTypeEnumJsonConverter))]
        public ProductTypeEnum TypeOfUniversity
        {
            get;
            set;
        }

        /// <summary>
        /// Custom converter to allow empty string values in existing JSON to map to Undefined.
        /// Also serializes enum values as their string names.
        /// Implemented with fast-fail style and without else statements.
        /// </summary>
        public class ProductTypeEnumJsonConverter : JsonConverter<ProductTypeEnum>
        {
            /// <summary>
            /// Reads a <see cref="ProductTypeEnum"/> value from JSON. Supports string names
            /// and numeric representations. Empty or invalid inputs map to <see cref="ProductTypeEnum.Undefined"/>.
            /// </summary>
            public override ProductTypeEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    string S = reader.GetString();

                    if (string.IsNullOrWhiteSpace(S))
                    {
                        return ProductTypeEnum.Undefined;
                    }

                    // Prefer numeric parsing first for explicit numeric strings so we can validate
                    // that the numeric value corresponds to a defined enum. This avoids Enum.TryParse
                    // treating numeric strings as valid enum values when they are not defined.
                    if (int.TryParse(S, out int Numeric))
                    {
                        if (Enum.IsDefined(typeof(ProductTypeEnum), Numeric))
                        {
                            return (ProductTypeEnum)Numeric;
                        }

                        return ProductTypeEnum.Undefined;
                    }

                    // Fall back to parsing by name (case-insensitive)
                    if (Enum.TryParse<ProductTypeEnum>(S, true, out ProductTypeEnum Parsed))
                    {
                        return Parsed;
                    }

                    return ProductTypeEnum.Undefined;
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    bool Ok = reader.TryGetInt32(out int NumericValue);

                    if (Ok == false)
                    {
                        return ProductTypeEnum.Undefined;
                    }

                    if (Enum.IsDefined(typeof(ProductTypeEnum), NumericValue) == false)
                    {
                        return ProductTypeEnum.Undefined;
                    }

                    return (ProductTypeEnum)NumericValue;
                }

                return ProductTypeEnum.Undefined;
            }

            /// <summary>
            /// Writes the <see cref="ProductTypeEnum"/> as a string value in JSON.
            /// </summary>
            public override void Write(Utf8JsonWriter writer, ProductTypeEnum value, JsonSerializerOptions options)
            {
                // Use a switch expression for clarity and future extensibility.
                string Output = value switch
                {
                    ProductTypeEnum.Public => "Public",
                    ProductTypeEnum.Private => "Private",
                    ProductTypeEnum.Community => "Community",
                    ProductTypeEnum.Online => "Online",
                    ProductTypeEnum.Other => "Other",
                    _ => "Undefined",
                };

                writer.WriteStringValue(Output);
            }
        }
        
        // Total number of departments in the university.
        [Range(1, 500, ErrorMessage = "Number of departments must be between 1 and 500.")]
        public int NumberOfDepartments
        {
            get;
            set;
        }
        
        // Indicates whether the university offers online programs.
        [Required(ErrorMessage = "Please indicate whether the university offers online programs.")]
        public bool HasOnlinePrograms
        {
            get;
            set;
        }
        
        // List of campus locations belonging to the university.
        [MinLength(1, ErrorMessage = "At least one campus name must be provided.")]
        public List<string> Campuses
        {
            get;
            set;
        }

        /// <summary>
        /// Serialize this product to a JSON string.
        /// </summary>
        public override string ToString()
        {
            return JsonSerializer.Serialize<ProductModel>(this);
        }
    }
}