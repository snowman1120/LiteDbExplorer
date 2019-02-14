﻿using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace LiteDbExplorer.Controls.JsonViewer
{
    /// <summary>
	/// Allows producing foldings from a document based on braces.
	/// </summary>
	public class BraceFoldingStrategy
	{
		/// <summary>
		/// Gets/Sets the opening brace. The default value is '{'.
		/// </summary>
		public char OpeningBrace { get; set; }
		
		/// <summary>
		/// Gets/Sets the closing brace. The default value is '}'.
		/// </summary>
		public char ClosingBrace { get; set; }
		
		/// <summary>
		/// Creates a new BraceFoldingStrategy.
		/// </summary>
		public BraceFoldingStrategy()
		{
			this.OpeningBrace = '{';
			this.ClosingBrace = '}';
		}
		
		public void UpdateFoldings(FoldingManager manager, TextDocument document)
		{
            var newFoldings = CreateNewFoldings(document, out var firstErrorOffset);
			manager.UpdateFoldings(newFoldings, firstErrorOffset);
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			return CreateNewFoldings(document);
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
		{
			var newFoldings = new List<NewFolding>();
			
			var startOffsets = new Stack<int>();
			var lastNewLineOffset = 0;
			var openingBrace = this.OpeningBrace;
			var closingBrace = this.ClosingBrace;
			for (var i = 0; i < document.TextLength; i++) {
				var c = document.GetCharAt(i);
				if (c == openingBrace) {
					startOffsets.Push(i);
				} else if (c == closingBrace && startOffsets.Count > 0) {
					var startOffset = startOffsets.Pop();
					// don't fold if opening and closing brace are on the same line
					if (startOffset < lastNewLineOffset) {
						newFoldings.Add(new NewFolding(startOffset, i + 1));
					}
				} else if (c == '\n' || c == '\r') {
					lastNewLineOffset = i + 1;
				}
			}
			newFoldings.Sort((a,b) => a.StartOffset.CompareTo(b.StartOffset));
			return newFoldings;
		}
	}
}