function main() {
    $('#check_button').click(function () {
        const sourceRadix = 10;
        const destinationRadix = 2;

        const questionBox = $('#question');
        const question = questionBox.text();
        console.log(question);

        const nunmberBox = document.getElementById("nunmber_input");
        const bin = nunmberBox.value;
        const dec = parseInt(bin, destinationRadix);
        console.log(bin);
        console.log(dec);

        if (dec == question) {
            const msg1 = "<span class =\"result-correct\">" + bin + "<sub>(" + destinationRadix + ")</sub> is " + dec + "<sub>(" + sourceRadix + ")</sub></span>";
            const msg2 = concatinateStrings(msg1, $('#output').text());
            $('#output').text(msg2);
            console.log(msg1);
            console.log(msg2);
            const nextNumber = getRandomByte();
            questionBox.text(getRandomByte());
            console.log(nextNumber);
            nunmberBox.value = "";
        } else {
            const msg1 = "<span class =\"result-wrong\">" + bin + "<sub>(" + destinationRadix + ")</sub> is " + dec + "<sub>(" + sourceRadix + ")</sub></span>";
            const msg2 = concatinateStrings(msg1, $('#output').text());
            $('#output').text(msg2);
            console.log(msg1);
            console.log(msg2);
        }
        
        nunmberBox.focus();
    });
}

function getRandomByte() {
    return Math.floor(Math.random() * 256);
}

function concatinateStrings (new_string, existing_string) {
    if (new_string == "" || new_string == null) {
        return new_string
    } else {
        return new_string + "<br>" + existing_string
    }
}

$(function () {
    $('#question').text(getRandomByte());
    main();
});