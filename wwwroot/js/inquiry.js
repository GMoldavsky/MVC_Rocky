var dataTable;
$(document).ready(function () {
    //$('#myTable').DataTable(); //It load DataTable
    loadDataTable("GetInquiryList"); //url="GetInquiryList" is Action Method of API in the Controller. See InquiryController.cs and Index.cshtml
});

function loadDataTable(url) {
    dataTable = $('#tblData').DataTable({ //It load DataTable
        "ajax": { //Using Ajax Request
            "url": "/inquiry/" + url
        },
        "columns": [ //"id", "fullName", "phoneNumber", "email" as it named in the DataBase table InquiryHeader (camelcase)
            { "data": "id", "width": "10%" },
            { "data": "fullName", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "email", "width": "15%" },
            {//Display Button to navigate
                "data": "id", //"id" as parameter send when Button clicked
                "render": function(data) { // "render" to render HTML
                                // data is the id of inquiry
                    // Double tilt sign ` so we can use separate lines
                    //             "/Inquiry/Details/$(data)" = "/ControllerName/ActionName/Id parameter"
                    return ` 
                        <div class="text-center">
                            <a href="/Inquiry/Details/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                <i class="fas fa-edit"></i>
                            </a>
                        </div >`
                    ;
                },
                "width": "5%"
            }
        ]
    }); 
}
//If error: F12 to open  Console window to see errors 
// > Network 
// > select "GetInquiryList"(IActionResult call) 
// > Response > to see Fields names, they are Case Sensetive(camelcase)