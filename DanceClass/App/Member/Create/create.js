$(async function () {
    await populatePackages();
    registerEvent();
});

async function populatePackages() {
    try {
        var packages = await getPackages();
        console.log(packages);

        var select = $('#package');
        packages.forEach(package => {
            $('<option></option>', {
                "value": package.Id
            })
                .text(package.NumberOfSessions)
                .appendTo(select);
        });
    } catch (err) {
        console.log(err);
    }
}

async function getPackages() {
    return $.ajax({
        method: 'GET',
        async: true,
        url: '/Services/Package/GetAll'
    })
}

function registerEvent() {
    $('form#createMember').submit(async function (event) {
        event.preventDefault();
        var formData = covnertFormDataToDictionary($(this).serializeArray());
        console.log(formData);
        
        try {
            await createMember({
                Member: {
                    FullName: formData["name"],
                    Email: formData["email"],
                    PhoneNumber: formData["phone"],
                    IdentityNo: formData["identity"],
                    PackageId: formData["package"]
                }
            });
            alert('Create member successfully');
        } catch (err) {
            alert('Failed to create member');
            console.log(err);
        }
    });
}

function covnertFormDataToDictionary(formData) {
    return formData.reduce((prev, cur) => {
        prev[cur.name] = cur.value;
        return prev;
    }, {});
}

async function createMember(data) {
    return $.ajax({
        method: 'POST',
        async: true,
        url: '/Services/Members/Create',
        data
    })
}
