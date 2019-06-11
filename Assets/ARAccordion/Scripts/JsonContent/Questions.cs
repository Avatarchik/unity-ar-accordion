﻿using System.Collections.Generic;

namespace jsonObject
{
	public class Questions {
	  	public string questionText { get; set; }
      	public string correctAnswer { get; set; }
      	public string extraInformation { get; set; }
      	public IList<string> answers { get; set; }
	}
}