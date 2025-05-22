using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    public static class AilmentHandlerUtils
    {
        public static void HandleTurnStart(BattleTeam InTeam)
        {
            List<GridPawnUnit> GridUnits = TacticBattleManager.GetUnitsOnTeam(InTeam);
            foreach (GridPawnUnit unit in GridUnits)
            {
                if(unit)
                {
                    List<Ailment> ailments = unit.GetAilmentContainer().GetAilments();
                    foreach (Ailment currAilment in ailments)
                    {
                        if(currAilment)
                        {
                            AilmentExecutionInfo StartExecution = currAilment.m_ExecuteOnStartOfTurn;
                            HandleAilmentExecution(unit, StartExecution);
                        }
                    }

                    unit.GetAilmentContainer().IncrementAllAilments();
                    unit.GetAilmentContainer().CheckAilments();
                }
            }

            List<LevelCellBase> levelCells = TacticBattleManager.GetGrid().GetAllCells();
            foreach (LevelCellBase currLevelCell in levelCells)
            {
                if (currLevelCell)
                {
                    GridPawnUnit unit = currLevelCell.GetUnitOnCell();
                    
                    AilmentContainer ailmentContainer = currLevelCell.GetAilmentContainer();
                    if (ailmentContainer)
                    {
                        List<AilmentContainedData> ailments = ailmentContainer.GetAllAilmentContainerData();

                        for (int i = 0; i < ailments.Count; i++)
                        {
                            AilmentContainedData currAilment = ailments[i];
                            if (currAilment.m_ailment)
                            {
                                AilmentExecutionInfo StartExecution = currAilment.m_ailment.m_ExecuteOnStartOfTurn;
                                HandleCellAilmentExecution(currAilment.m_AssociatedCell, StartExecution);

                                GridPawnUnit Caster = currAilment.m_CastedBy;
                                if (Caster)//If there is a caster, then check if it's their turn to increment the ailment
                                {
                                    bool bShouldIncrementAilment = InTeam == Caster.GetTeam();
                                    if (bShouldIncrementAilment)
                                    {
                                        ailmentContainer.IncrementAilment(currAilment);
                                    }
                                }
                                else//If there was no caster, check if it was friendly
                                {
                                    if (InTeam == BattleTeam.Friendly)
                                    {
                                        ailmentContainer.IncrementAilment(currAilment);
                                    }
                                }
                            }
                        }
                        
                        ailmentContainer.CheckAilments();
                    }
                }
            }
        }

        public static void HandleTurnEnd(BattleTeam InTeam)
        {
            List<GridPawnUnit> GridUnits = TacticBattleManager.GetUnitsOnTeam(InTeam);
            foreach (GridPawnUnit unit in GridUnits)
            {
                if (unit)
                {
                    List<Ailment> ailments = unit.GetAilmentContainer().GetAilments();
                    foreach (Ailment currAilment in ailments)
                    {
                        if (currAilment)
                        {
                            AilmentExecutionInfo EndExecution = currAilment.m_ExecuteOnEndOfTurn;
                            HandleAilmentExecution(unit, EndExecution);
                        }
                    }
                }
            }

            List<LevelCellBase> levelCells = TacticBattleManager.GetGrid().GetAllCells();
            foreach (LevelCellBase currLevelCell in levelCells)
            {
                if (currLevelCell)
                {
                    GridPawnUnit unit = currLevelCell.GetUnitOnCell();
                    if(unit && unit.GetTeam() == InTeam)
                    {
                        AilmentContainer ailmentContainer = currLevelCell.GetAilmentContainer();
                        if (ailmentContainer)
                        {
                            List<AilmentContainedData> ailments = ailmentContainer.GetAllAilmentContainerData();
                            foreach (AilmentContainedData currAilment in ailments)
                            {
                                if (currAilment.m_ailment)
                                {
                                    AilmentExecutionInfo EndExecution = currAilment.m_ailment.m_ExecuteOnEndOfTurn;
                                    HandleCellAilmentExecution(currAilment.m_AssociatedCell, EndExecution);
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void HandleUnitOnCell(GridPawnUnit InUnit, LevelCellBase InCell)
        {
            if (InCell && InUnit)
            {
                AilmentContainer ailmentContainer = InCell.GetAilmentContainer();
                if (ailmentContainer)
                {
                    List<AilmentContainedData> ailments = ailmentContainer.GetAllAilmentContainerData();
                    foreach (AilmentContainedData currAilment in ailments)
                    {
                        if (currAilment.m_ailment)
                        {
                            CellAilment cellAilment = currAilment.m_ailment as CellAilment;
                            if(cellAilment)
                            {
                                HandleAilmentExecution(InUnit, cellAilment.m_ExecuteOnUnitOver);
                            }
                        }
                    }
                }
            }
        }

        static void HandleAilmentExecution(GridPawnUnit InUnit, AilmentExecutionInfo InAilmentExecution)
        {
            foreach (AbilityParam abilityParam in InAilmentExecution.m_Params)
            {
                abilityParam.ApplyTo(null, InUnit);
            }

            foreach (AbilityParticle abilityParticle in InAilmentExecution.m_SpawnOnReciever)
            {
                Vector3 pos = InUnit.transform.position;
                Quaternion rot = abilityParticle.transform.rotation;
                AbilityParticle CreatedAbilityParticle = GameObject.Instantiate(abilityParticle.gameObject, pos, rot).GetComponent<AbilityParticle>();
                CreatedAbilityParticle.Setup(null, null, InUnit.GetCell());
            }

            AudioClip audioClip = InAilmentExecution.m_AudioClip;
            if (audioClip)
            {
                AudioPlayData audioData = new AudioPlayData(audioClip);
                AudioHandler.PlayAudio(audioData, InUnit.gameObject.transform.position);
            }
        }

        static void HandleCellAilmentExecution(LevelCellBase InCell, AilmentExecutionInfo InAilmentExecution)
        {
            GridPawnUnit unitOnCell = InCell.GetUnitOnCell();
            if(unitOnCell)
            {
                foreach (AbilityParam abilityParam in InAilmentExecution.m_Params)
                {
                    abilityParam.ApplyTo(null, unitOnCell);
                }

                foreach (AbilityParticle abilityParticle in InAilmentExecution.m_SpawnOnReciever)
                {
                    Vector3 pos = unitOnCell.transform.position;
                    Quaternion rot = abilityParticle.transform.rotation;
                    AbilityParticle CreatedAbilityParticle = GameObject.Instantiate(abilityParticle.gameObject, pos, rot).GetComponent<AbilityParticle>();
                    CreatedAbilityParticle.Setup(null, null, unitOnCell.GetCell());
                }

                AudioClip audioClip = InAilmentExecution.m_AudioClip;
                if (audioClip)
                {
                    AudioPlayData audioData = new AudioPlayData(audioClip);
                    AudioHandler.PlayAudio(audioData, InCell.gameObject.transform.position);
                }
            }
        }
    }
}
