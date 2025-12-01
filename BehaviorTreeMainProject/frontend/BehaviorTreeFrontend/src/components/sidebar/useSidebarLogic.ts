import { useCallback, useMemo, useRef, useState } from "react";
import {
  ADD_LABELS,
  DEFAULT_DATA,
  DEFAULT_ORDER,
  DEFAULT_TITLES,
  ACTION_INSTANCES_KEY,
  ACTION_TYPES_KEY,
  PARAM_INSTANCES_KEY,
  PARAM_TYPES_KEY,
  PREDICATE_INSTANCES_KEY,
  PREDICATE_TYPES_KEY,
} from "./utils/constants";
import {
  cloneActionInstance,
  cloneActionType,
  cloneParameterInstance,
  cloneParameterType,
  clonePredicateInstance,
  clonePredicateType,
  createEmptyActionInstance,
  createEmptyActionType,
  createEmptyParameterInstance,
  createEmptyParameterType,
  createEmptyPredicateInstance,
  createEmptyPredicateType,
  createEmptyStructuredItem,
  generateItemId,
  generatePropertyId,
  reconcileInstanceValues,
} from "./utils/helpers";
import type {
  ActionInstance,
  ActionType,
  AppData,
  CategoryModalState,
  DataCategory,
  ModalState,
  ParameterInstance,
  ParameterType,
  PredicateInstance,
  PredicateType,
  SearchQueries,
  SidebarManager,
  StructuredItem,
} from "./utils/types";

/** describes the shared shape for modal controller state. */
interface ModalControllerState<TValue> {
  isOpen: boolean;
  mode: "add" | "edit";
  index: number | null;
  initialValue: TValue;
  revision: number;
}

/** exposes modal state along with the typical lifecycle helpers. */
interface ModalController<TValue> {
  state: ModalControllerState<TValue>;
  openAdd: (initialValue?: TValue) => void;
  openEdit: (index: number, initialValue: TValue) => void;
  close: () => void;
}

/**
 * provides a monotonically increasing revision number for modal resets.
 * @returns function that returns the next revision identifier per invocation
 */
const useRevisionGenerator = () => {
  const counterRef = useRef(0);

  return useCallback(() => {
    counterRef.current += 1;
    return counterRef.current;
  }, []);
};

/**
 * centralizes reusable modal state management helpers for add/edit flows.
 * @param createEmpty factory returning a blank value for the modal
 * @returns modal state alongside helpers for opening and closing the modal
 */
const useModalController = <TValue,>(
  createEmpty: () => TValue
): ModalController<TValue> => {
  const nextRevision = useRevisionGenerator();
  const [state, setState] = useState<ModalControllerState<TValue>>(() => ({
    isOpen: false,
    mode: "add",
    index: null,
    initialValue: createEmpty(),
    revision: 0,
  }));

  /**
   * opens the modal in "add" mode with an optional initial value.
   * @param initialValue optional initial value to prefill the modal
   */
  const openAdd = (initialValue?: TValue) => {
    setState({
      isOpen: true,
      mode: "add",
      index: null,
      initialValue: initialValue ?? createEmpty(),
      revision: nextRevision(),
    });
  };

  /**
   * opens the modal in "edit" mode with the provided index and initial value.
   * @param index index of the item being edited
   * @param initialValue initial value to prefill the modal
   */
  const openEdit = (index: number, initialValue: TValue) => {
    setState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue,
      revision: nextRevision(),
    });
  };

  /**
   * closes the modal and resets its state.
   */
  const close = () => {
    setState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmpty(),
      revision: nextRevision(),
    });
  };

  return { state, openAdd, openEdit, close };
};

/**
 * normalizes type definitions by trimming fields and ensuring property ids.
 * @param value type definition being persisted
 * @param createEmpty factory providing a fallback structure for missing ids
 * @returns sanitized type definition ready for storage
 */
const normalizeType = (
  value: ParameterType,
  createEmpty: () => ParameterType
): ParameterType => ({
  ...value,
  id: value.id || createEmpty().id,
  name: value.name.trim(),
  type: value.type.trim(),
  properties: value.properties.map((property) => ({
    ...property,
    id: property.id || generatePropertyId(),
    name: property.name.trim(),
    valueType: property.valueType.trim(),
  })),
});

/**
 * builds the initial search-query map with empty strings per default category.
 * @returns initialized search query map keyed by category
 */
const createInitialSearchQueries = (): SearchQueries => {
  const initialEntries = DEFAULT_ORDER.map((key) => [key, ""] as const);
  return Object.fromEntries(initialEntries) as SearchQueries;
};

/**
 * central hook that encapsulates sidebar state, modal coordination, and item crud helpers.
 * @returns api for interacting with sidebar state and derived data
 */
export const useSidebarManager = (): SidebarManager => {
  const [data, setData] = useState<AppData>(() => ({ ...DEFAULT_DATA }));
  const [categoryTitles, setCategoryTitles] = useState<Record<string, string>>(
    () => ({ ...DEFAULT_TITLES })
  );
  const [categoryOrder, setCategoryOrder] = useState<string[]>(() => [
    ...DEFAULT_ORDER,
  ]);
  const [searchQueries, setSearchQueries] = useState<SearchQueries>(
    createInitialSearchQueries
  );

  const [categoryModalState, setCategoryModalState] =
    useState<CategoryModalState>({
      isOpen: false,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });

  const parameterTypeModal = useModalController<ParameterType>(
    createEmptyParameterType
  );
  const predicateTypeModal = useModalController<PredicateType>(
    createEmptyPredicateType
  );
  const actionTypeModal = useModalController<ActionType>(
    createEmptyActionType
  );

  const parameterInstanceModal = useModalController<ParameterInstance>(
    createEmptyParameterInstance
  );
  const predicateInstanceModal = useModalController<PredicateInstance>(
    createEmptyPredicateInstance
  );
  const actionInstanceModal = useModalController<ActionInstance>(
    createEmptyActionInstance
  );

  const parameterTypeModalState = parameterTypeModal.state;
  const predicateTypeModalState = predicateTypeModal.state;
  const actionTypeModalState = actionTypeModal.state;
  const parameterInstanceModalState = parameterInstanceModal.state;
  const predicateInstanceModalState = predicateInstanceModal.state;
  const actionInstanceModalState = actionInstanceModal.state;

  const [modalState, setModalState] = useState<ModalState>(() => ({
    isOpen: false,
    mode: "add",
    category: null,
    index: null,
    initialValue: createEmptyStructuredItem(),
  }));

  const parameterTypes = useMemo(() => {
    const entries = data[PARAM_TYPES_KEY] as ParameterType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);

  const predicateTypes = useMemo(() => {
    const entries = data[PREDICATE_TYPES_KEY] as PredicateType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);

  const actionTypes = useMemo(() => {
    const entries = data[ACTION_TYPES_KEY] as ActionType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);

  const parameterTypeMap = useMemo(() => {
    const map = new Map<string, ParameterType>();
    parameterTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [parameterTypes]);

  const predicateTypeMap = useMemo(() => {
    const map = new Map<string, PredicateType>();
    predicateTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [predicateTypes]);

  const actionTypeMap = useMemo(() => {
    const map = new Map<string, ActionType>();
    actionTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [actionTypes]);

  /**
   * Opens the "add item" modal for the given category.
   * Special handling for typed categories.
   *
   * @param category The category for which to add a new item.
   */
  const openAddModal = (category: DataCategory) => {
    if (category === PARAM_TYPES_KEY) {
      parameterTypeModal.openAdd();
      return;
    }

    if (category === PREDICATE_TYPES_KEY) {
      predicateTypeModal.openAdd();
      return;
    }

    if (category === ACTION_TYPES_KEY) {
      actionTypeModal.openAdd();
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      if (parameterTypes.length === 0) {
        window.alert("Create a parameter type before adding instances.");
        return;
      }

      const defaultType = parameterTypes[0];
      parameterInstanceModal.openAdd(
        createEmptyParameterInstance(defaultType)
      );
      return;
    }

    if (category === PREDICATE_INSTANCES_KEY) {
      if (predicateTypes.length === 0) {
        window.alert("Create a predicate type before adding instances.");
        return;
      }

      const defaultType = predicateTypes[0];
      predicateInstanceModal.openAdd(
        createEmptyPredicateInstance(defaultType)
      );
      return;
    }

    if (category === ACTION_INSTANCES_KEY) {
      if (actionTypes.length === 0) {
        window.alert("Create an action type before adding instances.");
        return;
      }

      const defaultType = actionTypes[0];
      actionInstanceModal.openAdd(createEmptyActionInstance(defaultType));
      return;
    }

    setModalState({
      isOpen: true,
      mode: "add",
      category,
      index: null,
      initialValue: createEmptyStructuredItem(),
    });
  };

  /**
   * opens the parameter type modal in edit mode with a cloned payload.
   * @param index index of the parameter type within the list
   * @param currentValue parameter type currently selected for editing
   */
  const openEditParameterType = (
    index: number,
    currentValue: ParameterType
  ) => {
    parameterTypeModal.openEdit(index, cloneParameterType(currentValue));
  };

  /**
   * opens the predicate type modal in edit mode with a cloned payload.
   * @param index index of the predicate type within the list
   * @param currentValue predicate type currently selected for editing
   */
  const openEditPredicateType = (
    index: number,
    currentValue: PredicateType
  ) => {
    predicateTypeModal.openEdit(index, clonePredicateType(currentValue));
  };

  /**
   * opens the action type modal in edit mode with a cloned payload.
   * @param index index of the action type within the list
   * @param currentValue action type currently selected for editing
   */
  const openEditActionType = (index: number, currentValue: ActionType) => {
    actionTypeModal.openEdit(index, cloneActionType(currentValue));
  };

  /**
   * opens the parameter instance modal in edit mode with reconciled values.
   * @param index index of the parameter instance within the list
   * @param currentValue instance currently selected for editing
   */
  const openEditParameterInstance = (
    index: number,
    currentValue: ParameterInstance
  ) => {
    const parameterType = parameterTypeMap.get(currentValue.typeId);
    const initialEntry = cloneParameterInstance(currentValue);

    if (parameterType) {
      initialEntry.type = parameterType.name;
      initialEntry.propertyValues = reconcileInstanceValues(
        parameterType,
        currentValue.propertyValues
      );
    }

    parameterInstanceModal.openEdit(index, initialEntry);
  };

  /**
   * opens the predicate instance modal in edit mode with reconciled values.
   * @param index index of the predicate instance within the list
   * @param currentValue instance currently selected for editing
   */
  const openEditPredicateInstance = (
    index: number,
    currentValue: PredicateInstance
  ) => {
    const predicateType = predicateTypeMap.get(currentValue.typeId);
    const initialEntry = clonePredicateInstance(currentValue);

    if (predicateType) {
      initialEntry.type = predicateType.name;
      initialEntry.propertyValues = reconcileInstanceValues(
        predicateType,
        currentValue.propertyValues
      );
    }

    predicateInstanceModal.openEdit(index, initialEntry);
  };

  /**
   * opens the action instance modal in edit mode with reconciled values.
   * @param index index of the action instance within the list
   * @param currentValue instance currently selected for editing
   */
  const openEditActionInstance = (
    index: number,
    currentValue: ActionInstance
  ) => {
    const actionType = actionTypeMap.get(currentValue.typeId);
    const initialEntry = cloneActionInstance(currentValue);

    if (actionType) {
      initialEntry.type = actionType.name;
      initialEntry.propertyValues = reconcileInstanceValues(
        actionType,
        currentValue.propertyValues
      );
    }

    actionInstanceModal.openEdit(index, initialEntry);
  };

  /**
   * Opens the edit modal for a generic structured item in a category.
   * Delegates to type-specific modals for typed categories.
   */
  const openEditModal = (
    category: DataCategory,
    index: number,
    currentValue: StructuredItem
  ) => {
    if (category === PARAM_TYPES_KEY) {
      openEditParameterType(index, currentValue as ParameterType);
      return;
    }

    if (category === PREDICATE_TYPES_KEY) {
      openEditPredicateType(index, currentValue as PredicateType);
      return;
    }

    if (category === ACTION_TYPES_KEY) {
      openEditActionType(index, currentValue as ActionType);
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      openEditParameterInstance(index, currentValue as ParameterInstance);
      return;
    }

    if (category === PREDICATE_INSTANCES_KEY) {
      openEditPredicateInstance(index, currentValue as PredicateInstance);
      return;
    }

    if (category === ACTION_INSTANCES_KEY) {
      openEditActionInstance(index, currentValue as ActionInstance);
      return;
    }

    setModalState({
      isOpen: true,
      mode: "edit",
      category,
      index,
      initialValue: { ...currentValue },
    });
  };

  /**
   * Closes the generic item modal without saving.
   */
  const closeModal = () => {
    setModalState((prev) => ({ ...prev, isOpen: false }));
  };

  /**
   * Saves a new or edited structured item from the generic modal.
   */
  const handleSaveFromModal = (value: StructuredItem) => {
    const categoryKey = modalState.category;
    if (!categoryKey) return;

    setData((prev) => {
      const existingItems = prev[categoryKey] ?? [];
      const nextItems = [...existingItems];
      const normalized: StructuredItem = {
        ...value,
        id: value.id || generateItemId(),
      };

      if (modalState.mode === "add") {
        nextItems.push(normalized);
      } else if (modalState.mode === "edit" && modalState.index !== null) {
        nextItems[modalState.index] = normalized;
      }

      return { ...prev, [categoryKey]: nextItems };
    });

    closeModal();
  };

  /**
   * closes the parameter type modal and restores its default state.
   */
  const closeParameterTypeModal = () => {
    parameterTypeModal.close();
  };

  /**
   * closes the predicate type modal and restores its default state.
   */
  const closePredicateTypeModal = () => {
    predicateTypeModal.close();
  };

  /**
   * closes the action type modal and restores its default state.
   */
  const closeActionTypeModal = () => {
    actionTypeModal.close();
  };

  /**
   * closes the parameter instance modal and restores its default state.
   */
  const closeParameterInstanceModal = () => {
    parameterInstanceModal.close();
  };

  /**
   * closes the predicate instance modal and restores its default state.
   */
  const closePredicateInstanceModal = () => {
    predicateInstanceModal.close();
  };

  /**
   * closes the action instance modal and restores its default state.
   */
  const closeActionInstanceModal = () => {
    actionInstanceModal.close();
  };

  /**
   * persists the provided parameter type and reconciles linked instances.
   * @param value parameter type collected from the modal form
   */
  const handleSaveParameterType = (value: ParameterType) => {
    const normalized = normalizeType(value, createEmptyParameterType);

    setData((prev) => {
      const existingTypes =
        (prev[PARAM_TYPES_KEY] as ParameterType[] | undefined) ?? [];
      const nextTypes = [...existingTypes];

      if (parameterTypeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (
        parameterTypeModalState.mode === "edit" &&
        parameterTypeModalState.index !== null
      ) {
        nextTypes[parameterTypeModalState.index] = normalized;
      }

      const existingInstances =
        (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
      const nextInstances = existingInstances.map((instance) => {
        if (instance.typeId !== normalized.id) {
          return instance;
        }

        return {
          ...instance,
          type: normalized.name,
          propertyValues: reconcileInstanceValues(
            normalized,
            instance.propertyValues
          ),
        };
      });

      return {
        ...prev,
        [PARAM_TYPES_KEY]: nextTypes,
        [PARAM_INSTANCES_KEY]: nextInstances,
      };
    });

    closeParameterTypeModal();
  };

  /**
   * persists the provided predicate type and reconciles linked instances.
   * @param value predicate type collected from the modal form
   */
  const handleSavePredicateType = (value: PredicateType) => {
    const normalized = normalizeType(value, createEmptyPredicateType);

    setData((prev) => {
      const existingTypes =
        (prev[PREDICATE_TYPES_KEY] as PredicateType[] | undefined) ?? [];
      const nextTypes = [...existingTypes];

      if (predicateTypeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (
        predicateTypeModalState.mode === "edit" &&
        predicateTypeModalState.index !== null
      ) {
        nextTypes[predicateTypeModalState.index] = normalized;
      }

      const existingInstances =
        (prev[PREDICATE_INSTANCES_KEY] as PredicateInstance[] | undefined) ??
        [];
      const nextInstances = existingInstances.map((instance) => {
        if (instance.typeId !== normalized.id) {
          return instance;
        }

        return {
          ...instance,
          type: normalized.name,
          propertyValues: reconcileInstanceValues(
            normalized,
            instance.propertyValues
          ),
        };
      });

      return {
        ...prev,
        [PREDICATE_TYPES_KEY]: nextTypes,
        [PREDICATE_INSTANCES_KEY]: nextInstances,
      };
    });

    closePredicateTypeModal();
  };

  /**
   * persists the provided action type and reconciles linked instances.
   * @param value action type collected from the modal form
   */
  const handleSaveActionType = (value: ActionType) => {
    const normalized = normalizeType(value, createEmptyActionType);

    setData((prev) => {
      const existingTypes =
        (prev[ACTION_TYPES_KEY] as ActionType[] | undefined) ?? [];
      const nextTypes = [...existingTypes];

      if (actionTypeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (
        actionTypeModalState.mode === "edit" &&
        actionTypeModalState.index !== null
      ) {
        nextTypes[actionTypeModalState.index] = normalized;
      }

      const existingInstances =
        (prev[ACTION_INSTANCES_KEY] as ActionInstance[] | undefined) ?? [];
      const nextInstances = existingInstances.map((instance) => {
        if (instance.typeId !== normalized.id) {
          return instance;
        }

        return {
          ...instance,
          type: normalized.name,
          propertyValues: reconcileInstanceValues(
            normalized,
            instance.propertyValues
          ),
        };
      });

      return {
        ...prev,
        [ACTION_TYPES_KEY]: nextTypes,
        [ACTION_INSTANCES_KEY]: nextInstances,
      };
    });

    closeActionTypeModal();
  };

  /**
   * stores the provided parameter instance after validating its type binding.
   * @param value parameter instance captured from the modal form
   */
  const handleSaveParameterInstance = (value: ParameterInstance) => {
    const parameterType = parameterTypeMap.get(value.typeId);
    if (!parameterType) {
      window.alert("Select a valid parameter type.");
      return;
    }

    const sanitizedValues = parameterType.properties.reduce<
      Record<string, string>
    >((acc, property) => {
      const rawValue = value.propertyValues?.[property.id] ?? "";
      acc[property.id] = rawValue.trim();
      return acc;
    }, {});

    const normalized: ParameterInstance = {
      ...value,
      id: value.id || createEmptyParameterInstance().id,
      name: value.name.trim(),
      type: parameterType.name,
      typeId: parameterType.id,
      propertyValues: sanitizedValues,
    };

    setData((prev) => {
      const existing =
        (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
      const next = [...existing];

      if (parameterInstanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        parameterInstanceModalState.mode === "edit" &&
        parameterInstanceModalState.index !== null
      ) {
        next[parameterInstanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [PARAM_INSTANCES_KEY]: next,
      };
    });

    closeParameterInstanceModal();
  };

  /**
   * stores the provided predicate instance after validating its type binding.
   * @param value predicate instance captured from the modal form
   */
  const handleSavePredicateInstance = (value: PredicateInstance) => {
    const predicateType = predicateTypeMap.get(value.typeId);
    if (!predicateType) {
      window.alert("Select a valid predicate type.");
      return;
    }

    const sanitizedValues = predicateType.properties.reduce<
      Record<string, string>
    >((acc, property) => {
      const rawValue = value.propertyValues?.[property.id] ?? "";
      acc[property.id] = rawValue.trim();
      return acc;
    }, {});

    const normalized: PredicateInstance = {
      ...value,
      id: value.id || createEmptyPredicateInstance().id,
      name: value.name.trim(),
      type: predicateType.name,
      typeId: predicateType.id,
      propertyValues: sanitizedValues,
      isNegated: value.isNegated ?? false,
    };

    setData((prev) => {
      const existing =
        (prev[PREDICATE_INSTANCES_KEY] as PredicateInstance[] | undefined) ??
        [];
      const next = [...existing];

      if (predicateInstanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        predicateInstanceModalState.mode === "edit" &&
        predicateInstanceModalState.index !== null
      ) {
        next[predicateInstanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [PREDICATE_INSTANCES_KEY]: next,
      };
    });

    closePredicateInstanceModal();
  };

  /**
   * stores the provided action instance after validating its type binding.
   * @param value action instance captured from the modal form
   */
  const handleSaveActionInstance = (value: ActionInstance) => {
    const actionType = actionTypeMap.get(value.typeId);
    if (!actionType) {
      window.alert("Select a valid action type.");
      return;
    }

    const sanitizedValues = actionType.properties.reduce<
      Record<string, string>
    >((acc, property) => {
      const rawValue = value.propertyValues?.[property.id] ?? "";
      acc[property.id] = rawValue.trim();
      return acc;
    }, {});

    const normalized: ActionInstance = {
      ...value,
      id: value.id || createEmptyActionInstance().id,
      name: value.name.trim(),
      type: actionType.name,
      typeId: actionType.id,
      propertyValues: sanitizedValues,
    };

    setData((prev) => {
      const existing =
        (prev[ACTION_INSTANCES_KEY] as ActionInstance[] | undefined) ?? [];
      const next = [...existing];

      if (actionInstanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        actionInstanceModalState.mode === "edit" &&
        actionInstanceModalState.index !== null
      ) {
        next[actionInstanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [ACTION_INSTANCES_KEY]: next,
      };
    });

    closeActionInstanceModal();
  };

  /**
   * removes an item from the requested category and cleans up dependent state.
   * @param category category key hosting the targeted item
   * @param index position of the item inside the category list
   */
  const handleDeleteItem = (category: DataCategory, index: number) => {
    if (!window.confirm("Delete this item?")) {
      return;
    }

    const removedEntry = (data[category] ?? [])[index];

    setData((prev) => {
      const existingItems = prev[category] ?? [];
      const nextItems = existingItems.filter((_, i) => i !== index);
      const nextData: AppData = { ...prev, [category]: nextItems };

      if (category === PARAM_TYPES_KEY && removedEntry) {
        const removedType = removedEntry as ParameterType;
        const existingInstances =
          (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
        nextData[PARAM_INSTANCES_KEY] = existingInstances.filter(
          (instance) => instance.typeId !== removedType.id
        );
      }

      if (category === PREDICATE_TYPES_KEY && removedEntry) {
        const removedType = removedEntry as PredicateType;
        const existingInstances =
          (prev[PREDICATE_INSTANCES_KEY] as PredicateInstance[] | undefined) ??
          [];
        nextData[PREDICATE_INSTANCES_KEY] = existingInstances.filter(
          (instance) => instance.typeId !== removedType.id
        );
      }

      if (category === ACTION_TYPES_KEY && removedEntry) {
        const removedType = removedEntry as ActionType;
        const existingInstances =
          (prev[ACTION_INSTANCES_KEY] as ActionInstance[] | undefined) ?? [];
        nextData[ACTION_INSTANCES_KEY] = existingInstances.filter(
          (instance) => instance.typeId !== removedType.id
        );
      }

      return nextData;
    });

    if (
      category === PARAM_INSTANCES_KEY &&
      parameterInstanceModalState.isOpen &&
      parameterInstanceModalState.index === index
    ) {
      closeParameterInstanceModal();
    }

    if (
      category === PREDICATE_INSTANCES_KEY &&
      predicateInstanceModalState.isOpen &&
      predicateInstanceModalState.index === index
    ) {
      closePredicateInstanceModal();
    }

    if (
      category === ACTION_INSTANCES_KEY &&
      actionInstanceModalState.isOpen &&
      actionInstanceModalState.index === index
    ) {
      closeActionInstanceModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      parameterTypeModalState.isOpen &&
      parameterTypeModalState.index === index
    ) {
      closeParameterTypeModal();
    }

    if (
      category === PREDICATE_TYPES_KEY &&
      predicateTypeModalState.isOpen &&
      predicateTypeModalState.index === index
    ) {
      closePredicateTypeModal();
    }

    if (
      category === ACTION_TYPES_KEY &&
      actionTypeModalState.isOpen &&
      actionTypeModalState.index === index
    ) {
      closeActionTypeModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      removedEntry &&
      parameterInstanceModalState.isOpen &&
      parameterInstanceModalState.initialValue.typeId ===
        (removedEntry as ParameterType).id
    ) {
      closeParameterInstanceModal();
    }

    if (
      category === PREDICATE_TYPES_KEY &&
      removedEntry &&
      predicateInstanceModalState.isOpen &&
      predicateInstanceModalState.initialValue.typeId ===
        (removedEntry as PredicateType).id
    ) {
      closePredicateInstanceModal();
    }

    if (
      category === ACTION_TYPES_KEY &&
      removedEntry &&
      actionInstanceModalState.isOpen &&
      actionInstanceModalState.initialValue.typeId ===
        (removedEntry as ActionType).id
    ) {
      closeActionInstanceModal();
    }
  };

  /**
   * opens the category modal in add mode with a blank payload.
   */
  const openCategoryModal = () => {
    setCategoryModalState({
      isOpen: true,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });
  };

  /**
   * opens the category modal prefilled for renaming the provided key.
   * @param categoryKey identifier of the category being renamed
   */
  const openRenameCategoryModal = (categoryKey: DataCategory) => {
    const currentTitle = categoryTitles[categoryKey] ?? categoryKey;
    setCategoryModalState({
      isOpen: true,
      mode: "edit",
      activeKey: categoryKey,
      value: {
        ...createEmptyStructuredItem(),
        name: currentTitle,
      },
    });
  };

  /**
   * closes the category modal and restores its default payload.
   */
  const closeCategoryModal = () => {
    setCategoryModalState({
      isOpen: false,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });
  };

  /**
   * transforms a category label into a unique slug identifier.
   * @param label human readable category label supplied by the user
   * @returns slugified category key guaranteed to be unique
   */
  const createCategoryKey = (label: string) => {
    const baseKey = label
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-+|-+$/g, "");

    const candidateBase = baseKey || "category";
    let candidate = candidateBase;
    let suffix = 1;
    const existingKeys = new Set([...categoryOrder, ...Object.keys(data)]);

    while (existingKeys.has(candidate)) {
      candidate = `${candidateBase}-${suffix++}`;
    }

    return candidate;
  };

  /**
   * persists category changes either by creating a new section or renaming.
   * @param value structured payload captured from the category modal
   */
  const handleSaveCategory = (value: StructuredItem) => {
    const displayName = value.name.trim();
    if (!displayName) {
      return;
    }

    if (categoryModalState.mode === "add") {
      const newKey = createCategoryKey(displayName);
      setData((prev) => ({ ...prev, [newKey]: [] }));
      setCategoryTitles((prev) => ({ ...prev, [newKey]: displayName }));
      setCategoryOrder((prev) => [...prev, newKey]);
      setSearchQueries((prev) => ({ ...prev, [newKey]: "" }));
    } else if (
      categoryModalState.mode === "edit" &&
      categoryModalState.activeKey
    ) {
      const activeKey = categoryModalState.activeKey;
      setCategoryTitles((prev) => ({
        ...prev,
        [activeKey]: displayName,
      }));
    }

    closeCategoryModal();
  };

  /**
   * removes an entire category and clears all nested data and modals.
   * @param categoryKey identifier of the category to remove
   */
  const handleDeleteCategory = (categoryKey: DataCategory) => {
    const displayName = categoryTitles[categoryKey] ?? categoryKey;
    if (
      !window.confirm(`Delete section "${displayName}" and all of its items?`)
    ) {
      return;
    }

    setCategoryOrder((prev) => prev.filter((key) => key !== categoryKey));
    setCategoryTitles((prev) => {
      const nextTitles = { ...prev };
      delete nextTitles[categoryKey];
      return nextTitles;
    });
    setData((prev) => {
      const nextData: AppData = { ...prev };
      delete nextData[categoryKey];
      return nextData;
    });
    setSearchQueries((prev) => {
      const nextQueries = { ...prev };
      delete nextQueries[categoryKey];
      return nextQueries;
    });
    setModalState((prev) =>
      prev.category === categoryKey
        ? {
            isOpen: false,
            mode: "add",
            category: null,
            index: null,
            initialValue: createEmptyStructuredItem(),
          }
        : prev
    );

    if (categoryKey === PARAM_TYPES_KEY) {
      closeParameterTypeModal();
    }

    if (categoryKey === PREDICATE_TYPES_KEY) {
      closePredicateTypeModal();
    }

    if (categoryKey === ACTION_TYPES_KEY) {
      closeActionTypeModal();
    }

    if (categoryKey === PARAM_INSTANCES_KEY) {
      closeParameterInstanceModal();
    }

    if (categoryKey === PREDICATE_INSTANCES_KEY) {
      closePredicateInstanceModal();
    }

    if (categoryKey === ACTION_INSTANCES_KEY) {
      closeActionInstanceModal();
    }

    if (categoryModalState.activeKey === categoryKey) {
      closeCategoryModal();
    }
  };

  /**
   * retrieves the items for a given category with a memoized lookup.
   * @param category category key used to look up stored items
   * @returns array of structured items for the provided category
   */
  const getItemsForCategory = useCallback(
    (category: DataCategory) => data[category] ?? [],
    [data]
  );

  /**
   * updates the search query string for the specified category.
   * @param category category key whose search filter should update
   * @param value new search string entered by the user
   */
  const handleSearchChange = (category: DataCategory, value: string) => {
    setSearchQueries((prev) => ({ ...prev, [category]: value }));
  };

  /**
   * resolves the localized add button label for the supplied category key.
   * @param category category key whose label should be retrieved
   * @returns translated add button label
   */
  const addLabelFor = (category: DataCategory) =>
    ADD_LABELS[category] ?? "Add Item";

  return {
    addLabelFor,
    categoryModal: categoryModalState,
    categoryOrder,
    categoryTitles,
    closeCategoryModal,
    closeParameterInstanceModal,
    closePredicateInstanceModal,
    closeActionInstanceModal,
    closeModal,
    closeParameterTypeModal,
    closePredicateTypeModal,
    closeActionTypeModal,
    getItemsForCategory,
    handleDeleteCategory,
    handleDeleteItem,
    handleSaveCategory,
    handleSaveFromModal,
    handleSaveParameterInstance,
    handleSavePredicateInstance,
    handleSaveActionInstance,
    handleSaveParameterType,
    handleSavePredicateType,
    handleSaveActionType,
    handleSearchChange,
    parameterInstanceModalState,
    predicateInstanceModalState,
    actionInstanceModalState,
    modalState,
    openAddModal,
    openCategoryModal,
    openEditModal,
    openRenameCategoryModal,
    parameterTypeMap,
    parameterTypes,
    predicateTypeMap,
    predicateTypes,
    actionTypeMap,
    actionTypes,
    searchQueries,
    parameterTypeModalState,
    predicateTypeModalState,
    actionTypeModalState,
  };
};
