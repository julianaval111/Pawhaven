document.addEventListener("DOMContentLoaded", function () {

    const searchInputs = document.querySelectorAll("[data-search-input]");

    searchInputs.forEach(function (input) {

        input.addEventListener("input", function () {

            const searchValue = input.value.toLowerCase().trim();

            const targetId = input.dataset.searchTarget;
            const tableBody = document.getElementById(targetId);

            if (!tableBody) {
                return;
            }

            const rows = tableBody.querySelectorAll("tr");

            rows.forEach(function (row) {

                const rowText = row.textContent.toLowerCase();

                if (rowText.includes(searchValue)) {
                    row.style.display = "";
                } else {
                    row.style.display = "none";
                }

            });

        });

    });

});