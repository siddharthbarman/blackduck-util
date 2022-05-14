using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SByteStream.BlackDuck
{
	public class AutoResetConsole : IDisposable
	{
		public AutoResetConsole(ConsoleColor foreColor)
		{
			m_initialForeColor = Console.ForegroundColor;
			m_initialBackColor = Console.BackgroundColor;
			Console.ForegroundColor = foreColor;
		}

		public void Dispose()
		{
			Console.ForegroundColor = m_initialForeColor;
			Console.BackgroundColor = m_initialBackColor;
		}

		private ConsoleColor m_initialForeColor;
		private ConsoleColor m_initialBackColor;		
	}
}
