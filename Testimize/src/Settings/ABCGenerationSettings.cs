// // <copyright file="ABCGenerationSettings.cs" company="Automate The Planet Ltd.">
// // Copyright 2025 Automate The Planet Ltd.
// // Licensed under the Apache License, Version 2.0 (the "License");
// // You may not use this file except in compliance with the License.
// // You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// // Unless required by applicable law or agreed to in writing,
// // software distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// // </copyright>
// // <author>Anton Angelov</author>
// // <site>https://automatetheplanet.com/</site>

using Testimize.OutputGenerators;

namespace Testimize;
public class ABCGenerationSettings
{
    public int TotalPopulationGenerations { get; set; } = 20;
    public double MutationRate { get; set; } = 0.3;
    public double FinalPopulationSelectionRatio { get; set; } = 0.5;
    public double EliteSelectionRatio { get; set; } = 0.5;
    public double OnlookerSelectionRatio { get; set; } = 0.1;
    public double ScoutSelectionRatio { get; set; } = 0.3;
    public bool EnableOnlookerSelection { get; set; } = true;
    public bool EnableScoutPhase { get; set; } = false;
    public bool EnforceMutationUniqueness { get; set; } = true;
    public double StagnationThresholdPercentage { get; set; } = 0.75;
    public double CoolingRate { get; set; } = 0.95;
    public bool AllowMultipleInvalidInputs { get; set; } = false;
    public ITestCaseOutputGenerator OutputGenerator { get; set; } = new NUnitTestCaseSourceOutputGenerator();

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + TotalPopulationGenerations.GetHashCode();
            hash = hash * 31 + MutationRate.GetHashCode();
            hash = hash * 31 + FinalPopulationSelectionRatio.GetHashCode();
            hash = hash * 31 + EliteSelectionRatio.GetHashCode();
            hash = hash * 31 + OnlookerSelectionRatio.GetHashCode();
            hash = hash * 31 + ScoutSelectionRatio.GetHashCode();
            hash = hash * 31 + EnableOnlookerSelection.GetHashCode();
            hash = hash * 31 + EnableScoutPhase.GetHashCode();
            hash = hash * 31 + StagnationThresholdPercentage.GetHashCode();
            hash = hash * 31 + AllowMultipleInvalidInputs.GetHashCode();
            hash = hash * 31 + (OutputGenerator?.GetType().GetHashCode() ?? 0); // Using type hash for OutputGenerator
            return hash;
        }
    }

    public override string ToString()
    {
        return $"HybridArtificialBeeColonyConfig: " +
               $"TotalPopulationGenerations={TotalPopulationGenerations}, " +
               $"MutationRate={MutationRate}, " +
               $"FinalPopulationSelectionRatio={FinalPopulationSelectionRatio}, " +
               $"EliteSelectionRatio={EliteSelectionRatio}, " +
               $"OnlookerSelectionRatio={OnlookerSelectionRatio}, " +
               $"ScoutSelectionRatio={ScoutSelectionRatio}, " +
               $"EnableOnlookerSelection={EnableOnlookerSelection}, " +
               $"EnableScoutPhase={EnableScoutPhase}, " +
               $"EnforceMutationUniqueness={EnforceMutationUniqueness}, " +
               $"StagnationThresholdPercentage={StagnationThresholdPercentage}, " +
               $"CoolingRate={CoolingRate}, " +
               $"AllowMultipleInvalidInputs={AllowMultipleInvalidInputs}, ";
    }
}
