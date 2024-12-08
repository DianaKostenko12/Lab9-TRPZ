using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab9
{
    public partial class Form1 : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
        private string query = "SELECT * FROM Book";
        private bool isChangesSaved = true;
        private BookStoreDbEntities db;
        private SqlDataAdapter adapter;
        private DataSet ds;
        public Form1()
        {
            InitializeComponent();
            SetBooks();
            BindInputsToDataGrid();
            db = new BookStoreDbEntities();
        }

        private void BindInputsToDataGrid()
        {
            BookIdBox.Enabled = false;
            BookIdBox.DataBindings.Add("Text", ds.Tables["Book"], "BookId");
            BookNameBox.DataBindings.Add("Text", ds.Tables["Book"], "Title", true);
            PublicationYearBox.DataBindings.Add("Text", ds.Tables["Book"], "YearOfPublication", true);
            CopiesCountBox.DataBindings.Add("Text", ds.Tables["Book"], "NumberOfCopies", true);
            PriceBox.DataBindings.Add("Text", ds.Tables["Book"], "Price", true);
            SupplierDateBox.DataBindings.Add("Text", ds.Tables["Book"], "DeliveryDate", true);
            CoverBox.DataBindings.Add("Text", ds.Tables["Book"], "Cover", true);
            AddPublisherBinding();
            AddAuthorBinding();
            AddGenreBinding();
            AddSupplierBinding();

            BindingContext[ds, "Book"].EndCurrentEdit();
        }

        public void AddPublisherBinding()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                adapter = new SqlDataAdapter("SELECT PublisherId, [Name] " +
                    "AS Publisher FROM Publisher", connection);

                adapter.Fill(ds, "Publisher");
                PublisherBox.DataSource = ds.Tables["Publisher"];
                PublisherBox.DisplayMember = "Publisher";
                PublisherBox.ValueMember = "PublisherId";

                PublisherBox.DataBindings.Add("SelectedValue", ds.Tables["Publisher"], "PublisherId", true);
            }
        }

        public void AddAuthorBinding()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                adapter = new SqlDataAdapter("SELECT AuthorId, [Surname] " +
                    "AS Author FROM Author", connection);

                adapter.Fill(ds, "Author");
                AuthorBox.DataSource = ds.Tables["Author"];
                AuthorBox.DisplayMember = "Author";
                AuthorBox.ValueMember = "AuthorId";

                AuthorBox.DataBindings.Add("SelectedValue", ds.Tables["Author"], "AuthorId", true);
            }
        }

        public void AddGenreBinding()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                adapter = new SqlDataAdapter("SELECT GenreId, [Name] " +
                    "AS Genre FROM Genre", connection);

                adapter.Fill(ds, "Genre");
                GenreBox.DataSource = ds.Tables["Genre"];
                GenreBox.DisplayMember = "Genre";
                GenreBox.ValueMember = "GenreId";

                GenreBox.DataBindings.Add("SelectedValue", ds.Tables["Genre"], "GenreId", true);
            }
        }

        public void AddSupplierBinding()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                adapter = new SqlDataAdapter("SELECT SupplierId, [Name] " +
                    "AS Supplier FROM Supplier", connection);

                adapter.Fill(ds, "Supplier");
                SupplierBox.DataSource = ds.Tables["Supplier"];
                SupplierBox.DisplayMember = "Supplier";
                SupplierBox.ValueMember = "SupplierId";

                SupplierBox.DataBindings.Add("SelectedValue", ds.Tables["Supplier"], "SupplierId", true);
            }
        }

        public void SetBooks()
        {
            dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AllowUserToAddRows = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    adapter = new SqlDataAdapter(query, connection);
                    ds = new DataSet();
                    adapter.Fill(ds, "Book");
                    dataGridView1.DataSource = ds.Tables[0];
                    dataGridView1.Columns["BookId"].ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while filling DataSet: {ex.Message}");
            }
            

            SubscribeOnChanges();
        }

        private void SubscribeOnChanges()
        {
            ds.Tables["Book"].RowChanged += delegate { isChangesSaved = false; };
            ds.Tables["Book"].RowDeleted += delegate { isChangesSaved = false; };
            ds.Tables["Book"].TableNewRow += delegate { isChangesSaved = false; };
        }

        private void AddBook(object sender, EventArgs e)
        {
            DataRow row = ds.Tables["Book"].NewRow();
            ds.Tables["Book"].Rows.Add(row);

            dataGridView1.ClearSelection();
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
        }

        private void RemoveBook(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                dataGridView1.Rows.Remove(row);
            }
        }

        private void SaveChanges(object sender, EventArgs e)
        {
            BindingContext[ds, "Book"].EndCurrentEdit();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    adapter = new SqlDataAdapter(query, connection);

                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    adapter.InsertCommand = new SqlCommand("AddBook", connection);
                    adapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@title", SqlDbType.NVarChar, 30, "Title"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@publicationYear", SqlDbType.Int, 0, "YearOfPublication"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@copyNum", SqlDbType.Int, 0, "NumberOfCopies"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@price", SqlDbType.Decimal, 0, "Price"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@delivDate", SqlDbType.Date, 0, "DeliveryDate"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@publisherId", SqlDbType.Int, 0, "PublisherId"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@authorId", SqlDbType.Int, 0, "AuthorId"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@genreId", SqlDbType.Int, 0, "GenreId"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@supplierId", SqlDbType.Int, 0, "SupplierId"));
                    adapter.InsertCommand.Parameters.Add(new SqlParameter("@cover", SqlDbType.NVarChar, 255, "Cover"));
                    SqlParameter parameter = adapter.InsertCommand.Parameters.Add("@bookId", SqlDbType.Int, 0, "BookId");
                    parameter.Direction = ParameterDirection.Output;

                    adapter.Update(ds.Tables["Book"]);
                }

                isChangesSaved = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Виникла помилка при збереженні змін:" + ex.Message);
            }
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            if (!isChangesSaved)
            {
                if (MessageBox.Show("Ви дійсно хочете вийти, не зберігши зміни?", "Увага",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
            else
                return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'bookStoreDbDataSet.Supplier' table. You can move, or remove it, as needed.
            this.supplierTableAdapter.Fill(this.bookStoreDbDataSet.Supplier);
            // TODO: This line of code loads data into the 'bookStoreDbDataSet.Genre' table. You can move, or remove it, as needed.
            this.genreTableAdapter.Fill(this.bookStoreDbDataSet.Genre);
            // TODO: This line of code loads data into the 'bookStoreDbDataSet.Publisher' table. You can move, or remove it, as needed.
            this.publisherTableAdapter.Fill(this.bookStoreDbDataSet.Publisher);
            // TODO: This line of code loads data into the 'bookStoreDbDataSet.Book' table. You can move, or remove it, as needed.
            this.bookTableAdapter.Fill(this.bookStoreDbDataSet.Book);
            // TODO: This line of code loads data into the 'bookStoreDbDataSet.Author' table. You can move, or remove it, as needed.
            this.authorTableAdapter.Fill(this.bookStoreDbDataSet.Author);

        }

        private void ValidateAndSaveChanges(object sender, EventArgs e)
        {
            this.Validate();
            this.bookBindingSource.EndEdit();
            this.bookTableAdapter.Update(bookStoreDbDataSet.Book);
        }

        private void ExecuteQuery(object sender, EventArgs e)
        {
            var bookAverageOrders = db.Orders
                    .GroupBy(o => o.BookId)
                    .Select(g => new
                    {
                        BookId = g.Key,
                        AverageOrders = g.Average(o => o.Quantity)
                    }).ToList();

            // Обчислення середньої кількості замовлень для кожного жанру
            var genreAverageOrders = db.Orders
                .Join(db.Book, o => o.BookId, b => b.BookId, (o, b) => new { o, b.GenreId })
                .GroupBy(x => x.GenreId)
                .Select(g => new
                {
                    GenreId = g.Key,
                    AverageGenreOrders = g.Average(x => x.o.Quantity)
                }).ToList();

            // З'єднання та фільтрація даних
            var query = from book in db.Book.ToList()
                        join bookAvg in bookAverageOrders on book.BookId equals bookAvg.BookId
                        join genreAvg in genreAverageOrders on book.GenreId equals genreAvg.GenreId
                        where bookAvg.AverageOrders > genreAvg.AverageGenreOrders
                        select new
                        {
                            BookTitle = book.Title,
                            AverageBookOrders = bookAvg.AverageOrders.HasValue
                                                ? Math.Round(bookAvg.AverageOrders.Value, 0)
                                                : 0, // Замінюємо null на 0
                            GenreName = db.Genre.FirstOrDefault(g => g.GenreId == book.GenreId)?.Name,
                            AverageGenreOrders = genreAvg.AverageGenreOrders.HasValue
                                                 ? Math.Round(genreAvg.AverageGenreOrders.Value, 0)
                                                 : 0 // Замінюємо null на 0
                        };

            dataGridView3.DataSource = query.ToList();
        }

    }
}
