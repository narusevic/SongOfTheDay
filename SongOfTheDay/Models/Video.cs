using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SongOfTheDay.Models
{
    public class Video
    {
        [Key]
        public int VideoId { get; set; }

        public string Link { get; set; }

        public string Name { get; set; }

        public string Information { get; set; }

        public string Image { get; set; }

        public DateTime Date { get; set; }

        public virtual ApplicationUser User { get; set; }

    }
}