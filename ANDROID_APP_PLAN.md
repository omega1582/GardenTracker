# Android App Implementation Plan

This plan is specifically structured for an AI agent (like Claude) to build the `GardenTracker.Android` application incrementally. 

## Agent Instructions
*   **Strict Adherence**: You MUST complete all tasks in a Goal and verify it is fully functional before moving to the next Goal. Do not jump ahead.
*   **Tech Stack**: Native Android, Kotlin, Jetpack Compose, Retrofit (for API), Navigation Compose, and ViewModels (MVVM).
*   **Backend API**: Run the backend using `./run-dev.sh` to test the app against `http://10.0.2.2:5280` (Android Emulator's alias to host localhost) or the machine's local IP.

---

## Goal 1: Gardens Dashboard
**Objective**: Allow the user to view and create Gardens.
- [ ] **Models**: Create DTOs (`GardenResponse`, `CreateGardenRequest`) and the `GardenApiService`.
- [ ] **Navigation**: Set up `NavHost` routing between Login and Home.
- [ ] **UI**: Build the `GardensListScreen` using a `LazyColumn`.
- [ ] **UI**: Implement a Floating Action Button (FAB) that opens an "Add Garden" bottom sheet.
- [ ] **Verification**: Verify that gardens fetched from the API render correctly, and creating a new garden immediately updates the list.

## Goal 2: Garden Details & Beds
**Objective**: Drill down into a Garden to manage its Beds.
- [ ] **Models**: Create DTOs (`BedResponse`, `CreateBedRequest`) and the `BedApiService`.
- [ ] **UI**: Build the `GardenDetailScreen`. It should display the garden's details and a list of associated Beds.
- [ ] **Navigation**: Implement routing from `GardensListScreen` to `GardenDetailScreen` (passing the `gardenId`).
- [ ] **UI**: Build the "Add Bed" dialog UI.
- [ ] **Verification**: Verify that tapping a garden navigates to its details, the beds load, and adding a bed works correctly.

## Goal 3: Core Workflow (Plantings)
**Objective**: Allow users to plant items in their beds.
- [ ] **Models**: Create DTOs (`PlantingResponse`, `PlantTypeResponse`, `PlantVarietyResponse`) and API services.
- [ ] **UI**: Build the `BedDetailScreen` to display current plantings.
- [ ] **UI**: Build the `AddPlantingScreen`. This requires fetching global `PlantTypes` and `PlantVarieties` for dropdown selection.
- [ ] **Verification**: Verify the complex flow of selecting a plant variety and successfully saving a planting to the bed.

## Goal 4: Quick-Action Logging (Harvests & Expenses)
**Objective**: Implement mobile-optimized actions for logging data while out in the garden.
- [ ] **Models**: Create DTOs and API services for `Harvests` and `Expenses`.
- [ ] **UI**: Add "Quick Action" buttons to the `BedDetailScreen` and `GardenDetailScreen` for "Log Harvest" and "Log Expense".
- [ ] **UI**: Build simplified input forms optimized for mobile keyboards (numeric keypads).
- [ ] **Verification**: Log a harvest and verify it appears in the backend database.

## Goal 5: Inventory Management
**Objective**: Allow users to check their seed/plant stash on the go.
- [ ] **Models**: Create DTOs and API services for `InventoryItem`.
- [ ] **UI**: Build a top-level `InventoryScreen` (accessible via Bottom Navigation) with Tabs for 'Seeds' and 'Plants'.
- [ ] **Verification**: Verify inventory lists render correctly.
