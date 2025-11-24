using System;

namespace ContosoCrafts.WebSite.Models
{
    /// <summary>
    /// Comment model representing a user's comment for a product.
    /// </summary>
    public class CommentModel
    {
        /// <summary>
        /// Default constructor initializes Id and defaults Comment to an empty string.
        /// </summary>
        public CommentModel()
        {
            Id = Guid.NewGuid().ToString();
            Comment = string.Empty;
        }
        
        // Gets or sets the unique identifier for this comment.
        public string Id
        {
            get;
            set;
        }
        
        // Gets or sets the textual content of the user's comment.
        public string Comment
        {
            get;
            set;
        }
    }
}
