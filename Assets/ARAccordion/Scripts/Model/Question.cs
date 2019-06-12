﻿using System.Collections.Generic;

namespace Model
{
	public class Question {
	  	public string questionText { get; set; }
      	public int correctAnswerId { get; set; }
      	public string extraInformation { get; set; }
      	public List<string> answers { get; set; }
	}
}