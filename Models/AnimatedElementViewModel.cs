namespace FrontendHelper.Models
{
    public class AnimatedElementViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Topic { get; set; }

        /// <summary>
        /// Относительный путь (или само имя файла) для тега &lt;video&gt; или &lt;img&gt; в случае GIF.
        /// Например: "myanimation.gif" или "clip.webm"
        /// </summary>
        public string Img { get; set; }

        /// <summary>
        /// Будем хранить признак «избранного» для текущего пользователя (true/false).
        /// </summary>
        public bool IsFavorited { get; set; }
    }
}
