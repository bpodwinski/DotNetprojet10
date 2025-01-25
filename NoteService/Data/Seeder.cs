using NoteService.Domain;
using MongoDB.Driver;

namespace NoteService.Data
{
    public static class MongoSeeder
    {
        public static async Task SeedNotesAsync(MongoDbContext context, ILogger logger)
        {
            try
            {
                if (!await context.Notes.Find(FilterDefinition<NoteDomain>.Empty).AnyAsync())
                {
                    var notes = new List<NoteDomain>
                    {
                        new() {
                            PatientId = 1,
                            Note = "Le patient est en bonne santé générale. Aucun symptôme significatif rapporté lors de l'examen. Toutes les analyses et les examens précédents ont montré des résultats normaux. Aucun historique de problèmes de santé majeurs ou de termes associés à des risques spécifiques.",
                            Date = DateTime.UtcNow.AddDays(-5)
                        },
                        new() {
                            PatientId = 2,
                            Note = "Analyse récente a révélé un taux de Hémoglobine A1C légèrement au-dessus des normes. Une présence modérée de Microalbumine a été détectée lors des tests urinaires. Le patient a signalé des épisodes passés de Vertiges, mais aucun symptôme récent. Aucun autre problème significatif n’a été noté.",
                            Date = DateTime.UtcNow.AddDays(-3)
                        },
                        new() {
                            PatientId = 3,
                            Note = "Le test sanguin montre une anomalie liée au Cholestérol. Le patient a signalé des épisodes de Vertiges au cours des dernières semaines. Des tests urinaires ont révélé des traces de Microalbumine. Conseillé d’éviter les déclencheurs potentiels de rechute et de suivre un régime adapté.",
                            Date = DateTime.UtcNow
                        },
                        new() {
                            PatientId = 4,
                            Note = "Les analyses sanguines révèlent une augmentation de l’Hémoglobine A1C. Des traces de Microalbumine ont été détectées lors des tests urinaires. Le patient a signalé des épisodes récurrents de Vertiges. L’examen physique a montré un Poids supérieur à la normale pour son âge et sa taille. Une Réaction allergique légère a été notée récemment. Le test de dépistage révèle un Cholestérol élevé. Le patient est une Fumeuse occasionnelle et a été conseillé d’arrêter complètement.",
                            Date = DateTime.UtcNow
                        }
                    };

                    await context.Notes.InsertManyAsync(notes);
                    logger.LogInformation("Default notes seeded successfully");
                }
                else
                {
                    logger.LogInformation("Notes collection already contains data. Seeding skipped");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the notes collection");
            }
        }
    }
}
