using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SongOfTheDay.Models
{
    public class Suggestion
    {
        [Key]
        public int SuggestionId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Opinion { get; set; }
        public string Image { get; set; }
        public string User { get; set; }}
}