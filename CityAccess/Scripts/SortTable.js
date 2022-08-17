function sortTable(n) {
    var table, rows, switching, i, x, y, shouldSwitch, dir, switchcount = 0;
    table = document.getElementById("MainTable");
    switching = true;
    //Set the sorting direction to ascending:
    dir = "asc";
    /*Make a loop that will continue until
    no switching has been done:*/
    while (switching) {
        //start by saying: no switching is done:
        switching = false;
        rows = table.rows;
        /*Loop through all table rows (except the
        first, which contains table headers):*/
        for (i = 1; i < (rows.length - 1); i++) {
            //start by saying there should be no switching:
            shouldSwitch = false;
            /*Get the two elements you want to compare,
            one from current row and one from the next:*/
            x = rows[i].getElementsByTagName("TD")[n];
            y = rows[i + 1].getElementsByTagName("TD")[n];
            /*check if the two rows should switch place,
            based on the direction, asc or desc:*/
            if (dir == "asc") {
                if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
                    //if so, mark as a switch and break the loop:
                    shouldSwitch = true;
                    break;
                }
            } else if (dir == "desc") {
                if (x.innerHTML.toLowerCase() < y.innerHTML.toLowerCase()) {
                    //if so, mark as a switch and break the loop:
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            /*If a switch has been marked, make the switch
            and mark that a switch has been done:*/
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            //Each time a switch is done, increase this count by 1:
            switchcount++;

            //Exception caused by the event regroup function
            if ((rows[i].hasAttribute("name") || rows[i].hasAttribute("id")) || (rows[i + 1].hasAttribute("name") || rows[i + 1].hasAttribute("id"))) {
                switchcount = switchcount - 1;
            }

        } else {
            /*If no switching has been done AND the direction is "asc",
            set the direction to "desc" and run the while loop again.*/
            if (switchcount == 0 && dir == "asc") {
                dir = "desc";
                switching = true;
            }
        }
    }
    RegroupEvents();
}

function sortTableN(n) {
    var table, rows, switching, i, x, y, shouldSwitch, dir, switchcount = 0;
    table = document.getElementById("MainTable");
    switching = true;
    dir = "asc";


    while (switching) {
        switching = false;
        rows = table.rows;
        for (i = 1; i < (rows.length - 1); i++) {
            shouldSwitch = false;
            x = rows[i].getElementsByTagName("TD")[n];
            y = rows[i + 1].getElementsByTagName("TD")[n];

            if (dir == "asc") {
                if (parseFloat(x.innerHTML) > parseFloat(y.innerHTML)) {
                    shouldSwitch = true;
                    break;
                }
            } else if (dir == "desc") {
                if (parseFloat(x.innerHTML) < parseFloat(y.innerHTML)) {
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;

                switchcount++;

            if ((rows[i].hasAttribute("name")  || rows[i].hasAttribute("id") ) || (rows[i + 1].hasAttribute("name")  || rows[i + 1].hasAttribute("id") )) {
                switchcount = switchcount - 1;
            }


        } else {
            if (switchcount == 0 && dir == "asc") {
                dir = "desc";
                switching = true;
            }
        }
    }
    
    RegroupEvents();
}

function sortTableDate(n) {
    var table, rows, switching, i, x, y, shouldSwitch, dir, switchcount = 0;
    table = document.getElementById("MainTable");
    switching = true;
    dir = "asc";
    while (switching) {
        switching = false;
        rows = table.rows;
        for (i = 1; i < (rows.length - 1); i++) {
            shouldSwitch = false;
            x = rows[i].getElementsByTagName("TD")[n].innerHTML;
            y = rows[i + 1].getElementsByTagName("TD")[n].innerHTML;

            x = String(x);
            y = String(y);

            var NX = x.split("-");
            NX = NX[2] + NX[1] + NX[0];
            x = NX;

            var NY = y.split("-");
            NY = NY[2] + NY[1] + NY[0];
            y = NY;

            //alert(NX);

            //if (x[2] === '-')
            //{
            //    var NX = x[6] + x[7] + x[8] + x[9] + x[2] + x[3] + x[4] + x[5] + x[0] + x[1];
            //    x = NX;
            //    alert(NX);
            //}
            //if (y[2] === '-') {
            //    var NY = y[6] + y[7] + y[8] + y[9] + y[2] + y[3] + y[4] + y[5] + y[0] + y[1];
            //    y = NY;
            //}


            if (dir == "asc") {
                if (x > y) {
                    shouldSwitch = true;
                    break;
                }
            } else if (dir == "desc") {
                if (x < y) {
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            switchcount++;
        } else {
            if (switchcount == 0 && dir == "asc") {
                dir = "desc";
                switching = true;
            }
        }
    }
}

function sortTableID(n) {
    var table, rows, switching, i, x, y, shouldSwitch, dir, switchcount = 0;
    table = document.getElementById("MainTable");
    switching = true;
    dir = "asc";
    while (switching) {
        switching = false;
        rows = table.rows;
        for (i = 1; i < (rows.length - 1); i++) {
            shouldSwitch = false;
            if ((rows[i].hasAttribute("name") === false && rows[i].hasAttribute("id") === false) && (rows[i + 1].hasAttribute("name") === false && rows[i + 1].hasAttribute("id") === false))
            {
                x = rows[i].getElementsByTagName("a")[n].text;
                y = rows[i + 1].getElementsByTagName("a")[n].text;

                x = parseInt(x);
                y = parseInt(y);

                if (dir == "asc") {
                    if (x > y) {
                        shouldSwitch = true;
                        break;
                    }
                } else if (dir == "desc") {
                    if (x < y) {
                        shouldSwitch = true;
                        break;
                    }
                }
            }
        }
        if (shouldSwitch) {
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            switchcount++;
        } else {
            if (switchcount == 0 && dir == "asc") {
                dir = "desc";
                switching = true;
            }
        }
    }
    RegroupEvents();
}

function sleep(milliseconds) {
    var start = new Date().getTime();
    for (var i = 0; i < 1e7; i++) {
        if ((new Date().getTime() - start) > milliseconds) {
            break;
        }
    }
}

function insertAfter(newNode, referenceNode) {
    //alert(referenceNode.nextElementSibling.innerHTML);
    referenceNode.parentNode.insertBefore(newNode, referenceNode.nextElementSibling);
}


function RegroupEvents() {

    var table, rows, switching, i, j, x, y, shouldSwitch, dir, switchcount = 0, fatherNode, check;
    table = document.getElementById("MainTable");

    switching = true;
    while (switching) {
        switching = false;
        rows = table.rows;
        //alert("aqui");
        for (i = 0; i < rows.length; i++) {
            shouldSwitch = false;

            //alert(rows[i].innerHTML);

            if (rows[i].hasAttribute("name"))
            {
                check = true;
                j = i - 1;
                while (check)
                {
                    if (rows[j].getAttribute("name") != rows[i].getAttribute("name"))
                    {

                        if (parseFloat(rows[j].getAttribute("id")) === parseFloat(rows[i].getAttribute("name"))) {
                            check = false;
                        }
                        else
                        {
                            shouldSwitch = true;
                            fatherNode = document.getElementById(rows[i].getAttribute("name"));
                            check = false

                        }
                    }
                    j = j - 1;
                }
            }
            if (shouldSwitch) {
                break;
            }
        }

        if (shouldSwitch)
        {
            insertAfter(rows[i], fatherNode);
            switching = true;
        } 
    }
}


