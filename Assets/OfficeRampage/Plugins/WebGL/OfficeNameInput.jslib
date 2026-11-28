mergeInto(LibraryManager.library, {
  OfficeNameOpen: function(receiver, value) {
    if (Module.officeNameInput) Module.officeNameInput.remove();
    var input = document.createElement('input');
    var target = UTF8ToString(receiver);
    input.type = 'text'; input.value = UTF8ToString(value); input.maxLength = 24;
    input.autocomplete = 'off'; input.spellcheck = false;
    input.setAttribute('aria-label', '이름');
    input.style.cssText = 'position:fixed;z-index:2147483647;box-sizing:border-box;background:#192c38;color:white;border:2px solid #40cbe0;border-radius:3px;padding:0 8px;font-family:"Noto Sans KR","Malgun Gothic",sans-serif;line-height:1.5;';
    var composing = false;
    function send() { SendMessage(target, 'OnBrowserName', JSON.stringify({text:input.value,composing:composing})); }
    input.addEventListener('compositionstart',function(){composing=true;send();});
    input.addEventListener('compositionend',function(){composing=false;send();});
    input.addEventListener('input',send);
    input.addEventListener('blur',function(){composing=false;send();});
    ['keydown','keyup','keypress'].forEach(function(name){input.addEventListener(name,function(e){e.stopPropagation();});});
    (document.fullscreenElement || document.body).appendChild(input);
    Module.officeNameInput=input;
    input.focus();input.select();
  },
  OfficeNameRect: function(x,y,w,h,sw,sh,fontSize) {
    var input=Module.officeNameInput;if(!input)return;
    var rect=Module.canvas.getBoundingClientRect();
    input.style.left=(rect.left+x/sw*rect.width)+'px';input.style.top=(rect.top+y/sh*rect.height)+'px';
    input.style.width=(w/sw*rect.width)+'px';input.style.height=(h/sh*rect.height)+'px';
    input.style.fontSize=(fontSize/sh*rect.height)+'px';
  },
  OfficeNameClose: function() {
    var input=Module.officeNameInput;if(!input)return;
    input.remove();Module.officeNameInput=null;Module.canvas.focus();
  }
});
