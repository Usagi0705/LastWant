﻿namespace prjCoreWebWantWant.ViewModels
{
    public class ForumPostReply
    {
        public int AccountId { get; set; }

        public int? ParentId { get; set; }

        public string? Title { get; set; }

        public string PostContent { get; set; } = null!;

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }

        public byte Status { get; set; }

    }
}
