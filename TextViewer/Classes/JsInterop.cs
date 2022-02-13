using Microsoft.JSInterop;

namespace TextViewer.Classes
{
    public class JsInterop
    {
        private readonly IJSRuntime js;
        public JsInterop(IJSRuntime js)
        {
            this.js = js;
        }
        public async ValueTask CommunicateAccess(string url)
        {
            var temp = await js.InvokeAsync<string>("GetIP", url);
        }

        public void Dispose()
        {
        }
    }
}
