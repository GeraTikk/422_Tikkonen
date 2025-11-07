using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace _422_Tikkonen.Pages
{
    public partial class DiagrammPage : Page
    {
        private Tikkonen_DB_PaymentEntities _context = new Tikkonen_DB_PaymentEntities();

        public DiagrammPage()
        {
            InitializeComponent();

            // Инициализация диаграммы
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            // Загрузка данных
            UserComboBox.ItemsSource = _context.User.ToList();
            DiagrammComboBox.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (UserComboBox.SelectedItem is User currentUser && DiagrammComboBox.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var categoriesList = _context.Category.ToList();
                foreach (var category in categoriesList)
                {
                    // Сумма платежей = Price * Num
                    decimal totalAmount = _context.Payment
                        .Where(p => p.UserID == currentUser.ID && p.CategoriID == category.ID)
                        .Sum(p => p.Price * p.Num);

                    currentSeries.Points.AddXY(category.Name, totalAmount);
                }
            }
        }

        private void ExcelButton_Click(object sender, RoutedEventArgs e)
        {
            var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

            // Общая сумма всех платежей (Price * Num)
            decimal grandTotal = allUsers.Sum(user =>
                user.Payment.Sum(payment => payment.Price * payment.Num));

            var application = new Excel.Application();
            application.SheetsInNewWorkbook = allUsers.Count;
            Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

            for (int i = 0; i < allUsers.Count; i++)
            {
                int startRowIndex = 1;
                Excel.Worksheet worksheet = application.Worksheets.Item[i + 1];
                worksheet.Name = allUsers[i].FIO;

                // Заголовки таблицы
                worksheet.Cells[1, 1] = "Дата платежа";
                worksheet.Cells[1, 2] = "Название";
                worksheet.Cells[1, 3] = "Категория";
                worksheet.Cells[1, 4] = "Цена";
                worksheet.Cells[1, 5] = "Количество";
                worksheet.Cells[1, 6] = "Сумма";

                // Форматирование заголовков
                Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 6]];
                columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                columnHeaderRange.Font.Bold = true;

                startRowIndex++;

                // Группируем платежи по категориям
                var userPaymentsByCategory = allUsers[i].Payment
                    .OrderBy(p => p.Date)
                    .GroupBy(p => p.Category)
                    .OrderBy(g => g.Key.Name);

                foreach (var categoryGroup in userPaymentsByCategory)
                {
                    // Заголовок категории
                    Excel.Range categoryHeaderRange = worksheet.Range[
                        worksheet.Cells[startRowIndex, 1],
                        worksheet.Cells[startRowIndex, 6]];
                    categoryHeaderRange.Merge();
                    categoryHeaderRange.Value = categoryGroup.Key.Name;
                    categoryHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    categoryHeaderRange.Font.Italic = true;
                    startRowIndex++;

                    // Данные платежей в категории
                    foreach (var payment in categoryGroup)
                    {
                        worksheet.Cells[startRowIndex, 1] = payment.Date.ToString("dd.MM.yyyy");
                        worksheet.Cells[startRowIndex, 2] = payment.Name;
                        worksheet.Cells[startRowIndex, 3] = payment.Category.Name;
                        worksheet.Cells[startRowIndex, 4] = payment.Price;
                        (worksheet.Cells[startRowIndex, 4] as Excel.Range).NumberFormat = "0.00";
                        worksheet.Cells[startRowIndex, 5] = payment.Num;
                        worksheet.Cells[startRowIndex, 6].Formula = $"=D{startRowIndex}*E{startRowIndex}";
                        (worksheet.Cells[startRowIndex, 6] as Excel.Range).NumberFormat = "0.00";
                        startRowIndex++;
                    }

                    // Итог по категории
                    Excel.Range categoryTotalRange = worksheet.Range[
                        worksheet.Cells[startRowIndex, 1],
                        worksheet.Cells[startRowIndex, 5]];
                    categoryTotalRange.Merge();
                    categoryTotalRange.Value = "ИТОГО по категории:";
                    categoryTotalRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                    worksheet.Cells[startRowIndex, 6].Formula =
                        $"=SUM(F{startRowIndex - categoryGroup.Count()}:F{startRowIndex - 1})";
                    worksheet.Cells[startRowIndex, 6].Font.Bold = true;
                    startRowIndex++;
                }

                // Границы таблицы
                Excel.Range rangeBorders = worksheet.Range[
                    worksheet.Cells[1, 1],
                    worksheet.Cells[startRowIndex - 1, 6]];
                rangeBorders.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Columns.AutoFit();
            }

            // Лист с общим итогом
            Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
            summarySheet.Name = "Общий итог";
            summarySheet.Cells[1, 1] = "Общий итог по всем пользователям:";
            summarySheet.Cells[1, 2] = grandTotal;
            (summarySheet.Cells[1, 2] as Excel.Range).NumberFormat = "0.00";

            Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
            summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
            summaryRange.Font.Bold = true;
            summarySheet.Columns.AutoFit();

            application.Visible = true;
        }

        private void WordButton_Click(object sender, RoutedEventArgs e)
        {
            var allUsers = _context.User.ToList();
            var allCategories = _context.Category.ToList();

            var application = new Word.Application();
            Word.Document document = application.Documents.Add();

            foreach (var user in allUsers)
            {
                // Заголовок пользователя
                Word.Paragraph userParagraph = document.Paragraphs.Add();
                Word.Range userRange = userParagraph.Range;
                userRange.Text = user.FIO;
                userParagraph.set_Style("Заголовок 1");
                userRange.InsertParagraphAfter();

                // Таблица платежей по категориям
                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count + 1, 2);
                paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                // Заголовки таблицы
                paymentsTable.Cell(1, 1).Range.Text = "Категория";
                paymentsTable.Cell(1, 2).Range.Text = "Сумма расходов";

                // Форматирование заголовков
                paymentsTable.Rows[1].Range.Font.Bold = 1;
                paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                // Данные таблицы
                for (int i = 0; i < allCategories.Count; i++)
                {
                    var currentCategory = allCategories[i];
                    paymentsTable.Cell(i + 2, 1).Range.Text = currentCategory.Name;

                    // Сумма платежей = Price * Num
                    decimal categoryTotal = user.Payment
                        .Where(p => p.CategoriID == currentCategory.ID)
                        .Sum(p => p.Price * p.Num);

                    paymentsTable.Cell(i + 2, 2).Range.Text = categoryTotal.ToString("N2") + " руб.";
                }

                document.Paragraphs.Add();

                // Самый дорогой платеж
                Payment maxPayment = user.Payment
                    .OrderByDescending(p => p.Price * p.Num)
                    .FirstOrDefault();

                if (maxPayment != null)
                {
                    Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                    Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                    maxPaymentRange.Text = $"Самый дорогой платеж: {maxPayment.Name} - {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. ({maxPayment.Date.ToString("dd.MM.yyyy")})";
                    maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                    maxPaymentRange.InsertParagraphAfter();
                }

                // Разрыв страницы между пользователями
                if (user != allUsers.Last())
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
            }

            application.Visible = true;
        }
    }
}