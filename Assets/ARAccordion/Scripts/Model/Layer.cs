﻿using System.Collections.Generic;

namespace Model
{
    public class Layer
    {
        public string information { get; set; }
        public List<Question> questions { get; set; }

        public Quiz quiz { get; set; }

    }
}