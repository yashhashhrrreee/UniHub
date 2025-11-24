using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ContosoCrafts.WebSite.Models;
using Microsoft.AspNetCore.Hosting;

namespace ContosoCrafts.WebSite.Services
{
    /// <summary>
    /// Service class to manage product data stored in a JSON file.
    /// </summary>
    public class JsonFileProductService
    {
        /// <summary>
        /// Initializes the product service, ensures necessary folders exist,
        /// and creates an empty products.json file if it does not exist.
        /// </summary>
        public JsonFileProductService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;

            var webRoot = WebRootPath;

            var imagesFolder = Path.Combine(webRoot, "images");

            if (DirectoryExists(imagesFolder))
            {
            }
            else
            {
                CreateDirectory(imagesFolder);
            }

            var dataFolder = Path.Combine(webRoot, "data");

            if (DirectoryExists(dataFolder))
            {
            }
            else
            {
                if (FileExists(dataFolder))
                {
                    DeleteFile(dataFolder);
                }

                CreateDirectory(dataFolder);
            }

            if (FileExists(JsonFileName))
            {
            }
            else
            {
                WriteAllText(JsonFileName, "[]");
            }
        }

        /// <summary>
        /// Web hosting environment.
        /// </summary>
        public IWebHostEnvironment WebHostEnvironment { get; }

        private string WebRootPath => WebHostEnvironment?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        private string JsonFileName => Path.Combine(WebRootPath, "data", "products.json");

        /// <summary>
        /// Checks if a file exists. Virtual for testing.
        /// </summary>
        protected virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Reads all text from a file. Virtual for testing.
        /// </summary>
        protected virtual string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Writes text to a file. Virtual for testing.
        /// </summary>
        protected virtual void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        /// <summary>
        /// Checks if a directory exists. Virtual for testing.
        /// </summary>
        protected virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Creates a directory. Virtual for testing.
        /// </summary>
        protected virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes a file, swallowing common exceptions.
        /// </summary>
        protected virtual void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
            }

            catch (UnauthorizedAccessException)
            {
            }
        }

        /// <summary>
        /// Retrieves all products from the JSON file.
        /// </summary>
        public IEnumerable<ProductModel> GetProducts()
        {
            if (FileExists(JsonFileName) == false)
            {
                return new List<ProductModel>();
            }

            try
            {
                string jsonText = ReadAllText(JsonFileName);

                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    return new List<ProductModel>();
                }

                ProductModel[] productsArray = JsonSerializer.Deserialize<ProductModel[]>(
                    jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (productsArray == null)
                {
                    return new List<ProductModel>();
                }

                return productsArray.ToList();
            }
            catch (JsonException)
            {
                return new List<ProductModel>();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<ProductModel>();
            }
            catch (IOException)
            {
                return new List<ProductModel>();
            }
        }

        /// <summary>
        /// Adds a rating to a product.
        /// </summary>
        public bool AddRating(string productId, int rating)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return false;
            }

            List<ProductModel> productsList = GetProducts().ToList();

            ProductModel productFound = productsList.FirstOrDefault(p => p.Id == productId);

            if (productFound == null)
            {
                return false;
            }

            if (productFound.Ratings == null)
            {
                productFound.Ratings = new int[] { rating };
                SaveData(productsList);
                return true;
            }

            List<int> ratingsList = productFound.Ratings.ToList();

            ratingsList.Add(rating);

            productFound.Ratings = ratingsList.ToArray();

            SaveData(productsList);

            return true;
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        public bool UpdateData(ProductModel product)
        {
            if (product == null)
            {
                return false;
            }

            List<ProductModel> productsList = GetProducts().ToList();

            ProductModel existingProduct = productsList.FirstOrDefault(p => p.Id.Equals(product.Id));

            if (existingProduct == null)
            {
                return false;
            }

            existingProduct.Title = product.Title;
            existingProduct.Description = product.Description;
            existingProduct.Url = product.Url;
            existingProduct.Image = product.Image;
            existingProduct.Location = product.Location;
            existingProduct.GraduateDegree = product.GraduateDegree;
            existingProduct.UnderGraduateDegree = product.UnderGraduateDegree;
            existingProduct.TypeOfUniversity = product.TypeOfUniversity;
            existingProduct.NumberOfDepartments = product.NumberOfDepartments;
            existingProduct.HasOnlinePrograms = product.HasOnlinePrograms;
            existingProduct.Campuses = product.Campuses;

            SaveData(productsList);

            return true;
        }

        /// <summary>
        /// Saves products to JSON file.
        /// </summary>
        private void SaveData(IEnumerable<ProductModel> products)
        {
            string dir = GetDataDirectory();

            if (string.IsNullOrWhiteSpace(dir))
            {
                return; // Exit gracefully if directory path cannot be determined
            }

            string jsonText = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });

            if (FileExists(JsonFileName))
            {
                DeleteFile(JsonFileName);
            }

            if (DirectoryExists(dir) == false)
            {
                if (FileExists(dir))
                {
                    DeleteFile(dir);
                }

                CreateDirectory(dir);
            }

            try
            {
                WriteAllText(JsonFileName, jsonText);
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        /// Returns data directory path.
        /// </summary>
        protected virtual string GetDataDirectory()
        {
            return Path.GetDirectoryName(JsonFileName);
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        public void CreateData(ProductModel product)
        {
            if (product == null)
            {
                return; // Exit gracefully if product is null
            }

            List<ProductModel> productsList = GetProducts().ToList();

            productsList.Add(product);

            SaveData(productsList);
        }

        /// <summary>
        /// Deletes a product by Id.
        /// </summary>
        public void DeleteData(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            List<ProductModel> productsList = GetProducts().ToList();

            ProductModel productToDelete = productsList.FirstOrDefault(p => p.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));

            if (productToDelete == null)
            {
                return;
            }

            DeleteLocalImageIfExists(productToDelete);

            productsList.Remove(productToDelete);

            SaveData(productsList);
        }

        /// <summary>
        /// Deletes the local image associated with a product.
        /// </summary>
        private void DeleteLocalImageIfExists(ProductModel product)
        {
            if (product == null)
            {
                return;
            }

            string image = product.Image;

            if (string.IsNullOrWhiteSpace(image))
            {
                return;
            }

            string normalized = image.Replace("\\", "/");

            if (normalized.StartsWith("/images/") == false)
            {
                return;
            }

            string relativePath = normalized.TrimStart('/');

            string webRoot = WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
            {
                return;
            }

            string physicalPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (FileExists(physicalPath))
            {
                DeleteFile(physicalPath);
            }
        }
    }
}
