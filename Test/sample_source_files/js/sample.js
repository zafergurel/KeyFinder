// another in 
if(typeof l10n === undefined) window.l10n = {getMessage:function(key) {return key;}};

var welcomeText = l10n.getMessage("Welcome to our site!")
, welcomeText = l10n.getMessage("Please read carefully...");


for (var i = 0; i < 100; i++) {
    i++;
}

var newText = l10n.getMessage("it's new");
newText = l10n.getMessage("change this");
for(var k=0; k < 10; k++) {
    console.log(l10n.getMessage('res_name_' + k));
}
