$('div .checkbox').click(function () {
    var checkedState = $(this).prop("checked")
    $(this)
        .parent('div')
        .children('.checkbox:checked')
        .prop("checked", false);

    $(this).prop("checked", checkedState);
});