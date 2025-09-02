// This function runs when the DOM is fully loaded.
document.addEventListener('DOMContentLoaded', function () {
    // This gets the button element from the HTML.
    const fetchDataBtn = document.getElementById('fetchDataBtn');
    // This gets the container element for the stock data.
    const stockDataContainer = document.getElementById('stockDataContainer');
    // This gets the input element for the stock symbol.
    const symbolInput = document.getElementById('symbolInput');

    // This adds a click event listener to the button.
    fetchDataBtn.addEventListener('click', function () {
        // This function is called when the button is clicked.
        fetchStockData();
    });

    function fetchStockData() {
        const symbol = symbolInput.value.trim() || 'IBM'; // Default to IBM if empty
        fetch(`/StockData?symbol=${encodeURIComponent(symbol)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                displayStockData(data);
            })
            .catch(error => {
                console.error('There has been a problem with your fetch operation:', error);
                stockDataContainer.innerHTML = '<p>Could not fetch stock data. Please try again later.</p>';
            });
    }

    // This function displays the stock data in a table.
    function displayStockData(data) {
        // This gets the time series data from the response.
        const timeSeries = data['Time Series (Daily)'];
        // This checks if the time series data exists.
        if (!timeSeries) {
            // This displays a message if no data is available.
            stockDataContainer.innerHTML = '<p>No stock data available.</p>';
            return;
        }

        // This gets the dates from the time series data.
        const dates = Object.keys(timeSeries);

        // This creates the HTML for the table.
        let tableHtml = '<table>';
        // This adds the table header.
        tableHtml += '<thead><tr><th>Date</th><th>Opening Price</th><th>Closing Price</th><th>Share Volume</th><th>Day-on-Day Change (%)</th><th>One-Month Change (%)</th><th>Change From Open (%)</th><th>Average Share Volume (Last 10 Days)</th></tr></thead>';
        // This starts the table body.
        tableHtml += '<tbody>';

        // This loops through each date in the time series data.
        dates.forEach(date => {
            const dayData = timeSeries[date];
            const change = Number(dayData['6. day_on_day_change_percent']);
            // Set background color based on value
            const bgColor = change < 0 ? 'background-color: #ffcccc;' : 'background-color: #ccffcc;';

            const oneMonthChange = Number(dayData['7. one_month_trailing']);
            // Set background color based on value
            const oneMonthBgColor = oneMonthChange < 0 ? 'background-color: #ffcccc;' : 'background-color: #ccffcc;';

            const ChangeFromOpenPercent = Number(dayData['8. change_from_open_percent']);
            // Set background color based on value
            const ChangeFromOpenPercentBgColor = ChangeFromOpenPercent < 0 ? 'background-color: #ffcccc;' : 'background-color: #ccffcc;';
            tableHtml += `
                <tr>
                    <td>${date}</td>
                    <td>${Number(dayData['1. open']).toFixed(2).toLocaleString()}</td>
                    <td>${Number(dayData['4. close']).toFixed(2).toLocaleString()}</td>
                    <td>${Number(dayData['5. volume']).toLocaleString()}</td>
                    <td style="${change !== 0 && !isNaN(change) ? bgColor : ''}">
                        ${change !== 0 && !isNaN(change) ? change.toFixed(2) + '%' : '-'}
                    </td>
                    <td style="${oneMonthChange !== 0 && !isNaN(oneMonthChange) ? oneMonthBgColor : ''}">
                        ${oneMonthChange !== 0 && !isNaN(oneMonthChange) ? oneMonthChange.toFixed(2) + '%' : '-'}
                    </td>
                    <td style="${ChangeFromOpenPercent !== 0 && !isNaN(ChangeFromOpenPercent) ? ChangeFromOpenPercentBgColor : ''}">
                        ${ChangeFromOpenPercent !== 0 && !isNaN(ChangeFromOpenPercent) ? ChangeFromOpenPercent.toFixed(2) + '%' : '-'}
                    </td>
                    <td>${dayData['9. average_db_volume'] > 0 ? Math.round(Number(dayData['9. average_db_volume'])).toLocaleString() : '-'}</td>
                </tr>
            `;
        });

        // This closes the table body.
        tableHtml += '</tbody>';
        // This closes the table.
        tableHtml += '</table>';

        // This sets the inner HTML of the container to the table.
        stockDataContainer.innerHTML = tableHtml;
    }
});
