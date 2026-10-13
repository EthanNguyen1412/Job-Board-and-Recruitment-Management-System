using System;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using JobBoardSystem.Models;

namespace JobBoardSystem.Infrastructure
{
    public static class PdfResumeBuilder
    {
        public static byte[] Build(CandidateProfile profile, string fullName, string email)
        {
            using (var stream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 48, 48, 48, 48);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                var baseFont = CreateFont();
                var titleFont = new Font(baseFont, 18, Font.BOLD, new BaseColor(14, 92, 166));
                var headingFont = new Font(baseFont, 13, Font.BOLD, new BaseColor(27, 32, 50));
                var bodyFont = new Font(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
                var mutedFont = new Font(baseFont, 10, Font.ITALIC, BaseColor.DARK_GRAY);

                document.Add(new Paragraph(ValueOrDash(fullName), titleFont));
                document.Add(new Paragraph(ValueOrDash(profile.Headline), headingFont));
                document.Add(new Paragraph(email ?? string.Empty, mutedFont));
                document.Add(Spacer());

                AddLine(document, "Phone", profile.Phone, bodyFont);
                AddLine(document, "Address", profile.Address, bodyFont);
                if (profile.DateOfBirth.HasValue)
                {
                    AddLine(document, "Date of birth", profile.DateOfBirth.Value.ToString("dd/MM/yyyy"), bodyFont);
                }

                AddSection(document, "Summary", profile.Summary, headingFont, bodyFont);

                document.Add(new Paragraph("Education", headingFont));
                if (profile.Educations == null || !profile.Educations.Any())
                {
                    document.Add(new Paragraph("No education records.", mutedFont));
                }
                else
                {
                    foreach (var item in profile.Educations.OrderByDescending(item => item.StartDate))
                    {
                        document.Add(new Paragraph(item.Degree + " - " + item.School, bodyFont));
                        var range = item.StartDate.ToString("MM/yyyy") + " - " + (item.EndDate.HasValue ? item.EndDate.Value.ToString("MM/yyyy") : "Present");
                        document.Add(new Paragraph((item.FieldOfStudy ?? string.Empty) + " | " + range, mutedFont));
                        if (!string.IsNullOrWhiteSpace(item.Description))
                        {
                            document.Add(new Paragraph(item.Description, bodyFont));
                        }
                    }
                }

                document.Add(Spacer());
                document.Add(new Paragraph("Work experience", headingFont));
                if (profile.WorkExperiences == null || !profile.WorkExperiences.Any())
                {
                    document.Add(new Paragraph("No work experience records.", mutedFont));
                }
                else
                {
                    foreach (var item in profile.WorkExperiences.OrderByDescending(item => item.StartDate))
                    {
                        document.Add(new Paragraph(item.JobTitle + " - " + item.CompanyName, bodyFont));
                        var end = item.IsCurrent || !item.EndDate.HasValue ? "Present" : item.EndDate.Value.ToString("MM/yyyy");
                        document.Add(new Paragraph(item.StartDate.ToString("MM/yyyy") + " - " + end, mutedFont));
                        if (!string.IsNullOrWhiteSpace(item.Description))
                        {
                            document.Add(new Paragraph(item.Description, bodyFont));
                        }
                    }
                }

                document.Add(Spacer());
                document.Add(new Paragraph("Skills", headingFont));
                if (profile.Skills == null || !profile.Skills.Any())
                {
                    document.Add(new Paragraph("No skills.", mutedFont));
                }
                else
                {
                    var skills = string.Join(", ", profile.Skills
                        .Where(item => item.Skill != null)
                        .Select(item => item.Skill.Name + " (" + item.Level.GetDisplayName() + ")"));
                    document.Add(new Paragraph(skills, bodyFont));
                }

                document.Add(Spacer());
                document.Add(new Paragraph("Languages", headingFont));
                if (profile.Languages == null || !profile.Languages.Any())
                {
                    document.Add(new Paragraph("No languages.", mutedFont));
                }
                else
                {
                    foreach (var item in profile.Languages)
                    {
                        document.Add(new Paragraph(item.LanguageName + " - " + item.Proficiency.GetDisplayName(), bodyFont));
                    }
                }

                document.Close();
                return stream.ToArray();
            }
        }

        private static BaseFont CreateFont()
        {
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            if (File.Exists(fontPath))
            {
                return BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }

            return BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        }

        private static void AddSection(Document document, string title, string content, Font headingFont, Font bodyFont)
        {
            document.Add(Spacer());
            document.Add(new Paragraph(title, headingFont));
            document.Add(new Paragraph(string.IsNullOrWhiteSpace(content) ? "Not provided." : content, bodyFont));
        }

        private static void AddLine(Document document, string label, string value, Font font)
        {
            document.Add(new Paragraph(label + ": " + ValueOrDash(value), font));
        }

        private static Paragraph Spacer()
        {
            return new Paragraph(" ") { SpacingAfter = 6f };
        }

        private static string ValueOrDash(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
